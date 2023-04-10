using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace FiniteStateMachine.EnemyState
{
    #region SelfDestruct
    public class SelfDestruct : State<EnemyCharacter>
    {
        private float selfDestructUpdateTime;
        private float selfDestructTime;
        public SelfDestruct(EnemyCharacter _owner, float _selfDestructTime) : base(_owner)
        {
            selfDestructTime = _selfDestructTime;
        }
        protected override void Enter()
        {
            if (owner.animator != null)
                owner.animator.CrossFade("EnemySelfDestruct", 0.3f, -1);
            selfDestructUpdateTime = 0;
            bDone = false;
        }

        protected override void Execute()
        {
            selfDestructUpdateTime += Time.deltaTime;
            if(selfDestructUpdateTime >= selfDestructTime)
            {
                selfDestructUpdateTime = 0;
                Done();

                if (!PhotonNetwork.IsMasterClient)
                    return;

                Character mainTower = (Character)PlayerManager.Instance.mainTowerCharacter;
                float distance_Sqr = Vector3.SqrMagnitude(owner.transform.position - mainTower.transform.position);

                if (distance_Sqr < Mathf.Pow(owner.attackRange, 2))
                {
                    mainTower.photonView.RPC("Damage", RpcTarget.All, owner.AttackDamage, owner.photonView.ViewID);
                }

                List<PlayerCharacter> playerCharacters = PlayerManager.Instance.GetPlayerCharacters();
                foreach (PlayerCharacter player in playerCharacters)
                {
                    if (!player.gameObject.activeSelf)
                        continue;

                    distance_Sqr = Vector3.SqrMagnitude(owner.transform.position - player.transform.position);
                    if (distance_Sqr < Mathf.Pow(owner.attackRange, 2))
                    {
                        player.photonView.RPC("Damage", RpcTarget.All, owner.AttackDamage, owner.photonView.ViewID);
                    }
                }
                owner.photonView.RPC("Death", RpcTarget.All, owner.photonView.ViewID, owner.effectOfAttackKey);
            }
        }

        protected override void Exit() { }
    }
    #endregion
    #region Move
    public class MoveToTarget : State<EnemyCharacter>
    {
        public MoveToTarget(EnemyCharacter _owner) : base(_owner) {}
        protected override void Enter()
        {
            if (owner.animator != null)
            {
                owner.animator.CrossFade("EnemyMove", 0.05f, -1);
                owner.animator.SetFloat("Speed", owner.MovementSpeed / 3);
            }
        }

        protected override void Execute() 
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                owner.transform.position = Vector3.MoveTowards(owner.transform.position, owner.GetMoveInterpolationValue(), Time.deltaTime * owner.MovementSpeed * 1.2f);
                owner.transform.rotation = Quaternion.Lerp(owner.transform.rotation, Quaternion.LookRotation(owner.networkDirection), Time.deltaTime * owner.turnSpeed);
                return;
            }
            Movement();
        }
        protected override void Exit() {}
        private void Movement()
        {
            Transform target = owner.GetTarget();
            if (target == null)
                return;

            Transform ownerTransform = owner.transform;
            float turnSpeed = owner.turnSpeed;
            float movementSpeed = owner.MovementSpeed;

            Vector3 direction = target.position - ownerTransform.position;
            direction.y = 0.0f;

            ownerTransform.rotation = Quaternion.Lerp(ownerTransform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * turnSpeed);
            ownerTransform.Translate(0, 0, movementSpeed * Time.deltaTime);
        }
    }
    #endregion
    #region MoveToTargetWithoutOverlapping
    public class MoveToTargetWithoutOverlapping : State<EnemyCharacter>
    {
        public MoveToTargetWithoutOverlapping(EnemyCharacter _owner) : base(_owner) {}
        protected override void Enter()
        {
            if (owner.animator != null)
            {
                owner.animator.CrossFade("EnemyMove", 0.05f, -1);
                owner.animator.SetFloat("Speed", owner.MovementSpeed / 3);
            }
        }
        protected override void Execute()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                owner.transform.position = Vector3.MoveTowards(owner.transform.position, owner.GetMoveInterpolationValue(), Time.deltaTime * owner.MovementSpeed * 1.2f);
                owner.transform.rotation = Quaternion.Lerp(owner.transform.rotation, Quaternion.LookRotation(owner.networkDirection), Time.deltaTime * owner.turnSpeed);
                return;
            }
            Movement(owner);
        }
        protected override void Exit() { }
        private void Movement(EnemyCharacter _owner)
        {
            Transform target = _owner.GetTarget();
            if (target == null)
                return;

            Transform ownerTransform = _owner.transform;
            float turnSpeed = _owner.turnSpeed;
            float movementSpeed = _owner.MovementSpeed;

            Vector3 direction = GetDirection(ownerTransform, target.position);

            ownerTransform.rotation = Quaternion.Lerp(ownerTransform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * turnSpeed);
            ownerTransform.Translate(0, 0, movementSpeed * Time.deltaTime);
        }
        private Vector3 GetDirection(Transform _ownerTransform, Vector3 _targetPosition)
        {
            Vector3 targetDirection = _targetPosition - _ownerTransform.position;
            targetDirection.y = 0.0f;
            Vector3 avoidDirection = Vector3.zero;
            int avoidCount = 0;
            List<Character> activityCharacters = CharacterManager.Instance.GetActivityCharacters();
            foreach (Character character in activityCharacters)
            {
                if (character.characterCode != CharacterCode.Enemy)
                    continue;
                Vector3 vector = _ownerTransform.position - character.transform.position;
                vector.y = 0.0f;
                float magnitude = vector.sqrMagnitude;
                if (magnitude <= Mathf.Pow(1f, 2))
                {
                    avoidCount++;
                    avoidDirection += vector;
                }
            }
            if (avoidCount > 0)
            {
                avoidDirection /= avoidCount;
            }
            return _ownerTransform.forward + (avoidDirection * 1.5f) + (targetDirection * 0.5f);
        }
    }
    #endregion
    #region Shooting
    public class Shooting : State<EnemyCharacter> 
    {
        private WeaponController weaponController;
        public Shooting(EnemyCharacter _owner, WeaponController _weaponController) : base(_owner)
        {
            weaponController = _weaponController;
        }
        protected override void Enter()
        {
            if (owner.animator != null)
            {
                owner.animator.CrossFade("EnemyIdle", 0.3f, -1);
            }
        }
        protected override void Execute()
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            Transform target = owner.GetTarget();
            if (target == null)
                return;

            Vector3 direction = (target.position - owner.transform.position).normalized;
            owner.transform.rotation = Quaternion.Lerp(owner.transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * owner.turnSpeed);
            
            float targetDistance = Vector3.Distance(owner.transform.position, target.position);
            weaponController.Attack(targetDistance);
        }
        protected override void Exit() { }
    }
    #endregion
    #region WaitForTime
    public class WaitForTime : State<EnemyCharacter>
    {
        private float waitTime;
        public WaitForTime(EnemyCharacter _owner, float _waitTime) : base(_owner)
        {
            waitTime = _waitTime;
        }
        protected override void Enter()
        {
            if (owner.animator != null)
            {
                owner.animator.CrossFade("EnemyIdle", 0.2f, -1);
            }
        }
        protected override void Execute()
        {
            waitTime -= Time.deltaTime;
            if (waitTime <= 0)
            {
                Done();
            }
        }
        protected override void Exit() {}
    }
    #endregion
}
