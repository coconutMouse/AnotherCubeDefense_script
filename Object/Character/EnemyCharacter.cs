using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using DamageNumbersPro;
using FiniteStateMachine;

public class EnemyCharacter : Character, IPunObservable, IManuallyInstantiatedNetworkObject
{
    [StringFromScriptableObject] 
    public string effectOfAttackKey;
    [HideInInspector]
    public Animator animator;
    [HideInInspector]
    public Vector3 networkPosition;
    [HideInInspector]
    public Vector3 networkDirection;
    public float turnSpeed;
    public float attackRange;

    [SerializeField]
    protected TargetCharacterCode targetCodesFromAttack;
    protected StateMachine<EnemyCharacter> stateMachine;
    protected State<EnemyCharacter> defaultState;
    protected Transform target;

    [SerializeField]
    private DamageNumber damageNumber;
    [SerializeField]
    private int enemyScore;
    [SerializeField]
    private float experiencePoint;
    private double sentServerTime;
    private float serverDelayValue;

    public override void OnEnable()
    {
        base.OnEnable();
        InitialSetting();
    }
    public virtual void InitialSetting()
    {
        hp = MaxHp;

        if (defaultState != null)
            stateMachine.SetDefaultState(defaultState);
    }
    public Vector3 GetMoveInterpolationValue()
    {
        serverDelayValue = Mathf.Abs((float)(PhotonNetwork.Time - sentServerTime));
        return networkPosition + networkDirection * MovementSpeed * serverDelayValue;
    }
    public void SetInterpolationValue(Vector3 _position, Vector3 _direction, double _sentServerTime)
    {
        networkPosition = _position;
        networkDirection = _direction;
        sentServerTime = _sentServerTime;
    }
    public Transform GetTarget()
    {
        return target;
    }

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        stateMachine = new StateMachine<EnemyCharacter>();
        SetupStateLayer();
    }
    protected virtual void SetupStateLayer() { }


    protected override void OnUpdate()
    {
        UpdateTarget();
        stateMachine.Execute();
    }
    protected override void OnFixedUpdate() { }

    protected override void OnDamage(float _damage, int _damageOwnerViewID)
    {
        damageNumber.Spawn(transform.position + Vector3.up * 2, (int)_damage);
    }
    protected override void OnDeath(int _killerViewID)
    {
        if (PlayerManager.Instance.IsPlayerCharacterViewID(_killerViewID))
        {
            PlayerManager.Instance.GetPlayerCharacter(_killerViewID).AddExperiencePoint(experiencePoint);
            GameSystem.KillMonsterScore += enemyScore;
        }
    }
    protected virtual void UpdateTarget()
    {
        Character mainTower = (Character)PlayerManager.Instance.mainTowerCharacter;
        Transform targetPlayer = mainTower.transform;
        List<PlayerCharacter> playerCharacters = PlayerManager.Instance.GetPlayerCharacters();
        float shortDistance_Sqr = Vector3.SqrMagnitude(transform.position - targetPlayer.position);

        foreach (PlayerCharacter player in playerCharacters)
        {
            if (!player.gameObject.activeSelf)
                continue;
            float distance_Sqr = Vector3.SqrMagnitude(transform.position - player.transform.position);
            if (shortDistance_Sqr > distance_Sqr)
            {
                shortDistance_Sqr = distance_Sqr;
                targetPlayer = player.transform;
            }
        }
        target = targetPlayer;
    }
    [PunRPC]
    protected void ActiveObject(Vector3 _position, Quaternion _rotation)
    {
        gameObject.transform.position = _position;
        gameObject.transform.rotation = _rotation;
        gameObject.SetActive(true);
    }

    void IManuallyInstantiatedNetworkObject.SendActiveTrue()
    {
        photonView.RPC("ActiveObject", RpcTarget.Others, transform.position, transform.rotation);
    }
    void IManuallyInstantiatedNetworkObject.SetInterpolation(double _sentServerTime)
    {
        SetInterpolationValue(transform.position, transform.forward, _sentServerTime);
    }
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.forward);
        }
        else
        {
            Vector3 position = (Vector3)stream.ReceiveNext();
            Vector3 direction = (Vector3)stream.ReceiveNext();
            SetInterpolationValue(position, direction, info.SentServerTime);
        }
    }
}
