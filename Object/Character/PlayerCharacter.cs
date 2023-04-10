using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;
using StorageKeySapce;

public enum AbilityStackCode
{
    MaxHp,
    HpRegen,
    AttackDamage,
    AttackDelayTime,
    MovementSpeed
}

public class PlayerCharacter : Character, IPunObservable
{
    [StringFromScriptableObject] public string effectOfLevelUpKey;
    public JobStorage jobStorage;
    public WeaponController weaponController;
    public GameObject shootDirectionImgObject;
    public float experienceIncreaseRate;
    public int spownPointNumber;
    public GameObject[] characterVisuals;
    public delegate void TriggerEvent<T>(T _value);
    public event TriggerEvent<int> OnAbilityStackPointChange;
    public event TriggerEvent<int> OnLevelChange;
    public event TriggerEvent<string[]> OnPromotionPossible;

    private Animator animator;
    private Rigidbody myRigidbody;
    private Vector3 moveDirection; 
    private Vector3 networkPosition;
    private Quaternion networkRotation;
    private MouseGroundArrow mouseGroundArrow;
    private JobData jobData;
    private string myJopId;
    private float turnSpeed = 5f;
    private float experiencePoint;
    private float maxExperiencePoint;
    private int level;
    private int abilityStackPoint;
    private int[] abilityStackCounts = new int[5];

    public int AbilityStackPoint
    {
        get => abilityStackPoint;
        set
        {
            abilityStackPoint = value;
            if (OnAbilityStackPointChange != null) 
                OnAbilityStackPointChange(abilityStackPoint);
        }
    }
    public int Level
    {
        get => level;
        set
        {
            level = value;
            if (OnLevelChange != null)
                OnLevelChange(level);
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        moveDirection = Vector3.zero;
    }
    public void AddExperiencePoint(float _point)
    {
        if (Level >= 30)
            return;
        experiencePoint += _point;
        if (CheckAvailableForLevelUp())
            LevelUp();

    }
    public float GetMaxExperiencePoint()
    {
        return maxExperiencePoint;
    }
    public float GetExperiencePoint()
    {
        return experiencePoint;
    }
    public void SettingAbilityStack()
    {
        MaxHp = jobData.maxHp + (jobData.maxHp * (abilityStackCounts[(int)AbilityStackCode.MaxHp] * 0.1f));
        hp = MaxHp;

        HpRegen = jobData.hpRegen + (jobData.hpRegen * (abilityStackCounts[(int)AbilityStackCode.HpRegen] * 0.1f));
        AttackDamage = jobData.attackDamage + (jobData.attackDamage * (abilityStackCounts[(int)AbilityStackCode.AttackDamage] * 0.1f));
        AttackDelayTime = jobData.attackDelayTime + (jobData.attackDelayTime * -(abilityStackCounts[(int)AbilityStackCode.AttackDelayTime] * 0.1f) * 0.5f);
        MovementSpeed = jobData.movementSpeed + (jobData.movementSpeed * (abilityStackCounts[(int)AbilityStackCode.MovementSpeed] * 0.1f) * 0.5f);
    
        animator.SetFloat("Speed", MovementSpeed / 5);
    }
    public int GetStackCount(AbilityStackCode _code)
    {
        return abilityStackCounts[(int)_code];
    }
    public Vector3 GetTargetDirection()
    {
        return mouseGroundArrow.GetArrowDirection();
    }
    public float GetTargetDistance()
    {
        return mouseGroundArrow.GetArrowDistanceFromPlayer();
    }
    public void SetToMyPlayerCharacter(MouseGroundArrow _mouseGroundArrow)
    {
        mouseGroundArrow = _mouseGroundArrow;
        shootDirectionImgObject.SetActive(true);
        gameObject.GetComponent<PlayerInput>().enabled = true;
    }
    public void SetCharacterVisual(int _characterVisualNumber)
    {
        for (int i = 0; i < characterVisuals.Length; i++)
        {
            if (i == _characterVisualNumber)
                characterVisuals[i].SetActive(true);
            else
                characterVisuals[i].SetActive(false);
        }
    }
    [PunRPC]
    public void ChangeJob(string _jobId)
    {
        if (!jobStorage.ContainsKey(_jobId))
        {
            Debug.LogError(jobStorage.name + " does not have a\"" + _jobId + "\" key.");
            return;
        }
        jobData = jobStorage.GetValue(_jobId);
        SettingAbilityStack();
        weaponController.SetWeaponsData(jobData);
    }
    [PunRPC]
    public void StackIncrease(AbilityStackCode _code)
    {
        if (abilityStackCounts[(int)_code] >= 10)
            return;

        AbilityStackPoint--;
        float stackVvalue = ++abilityStackCounts[(int)_code] * 0.1f;
        switch (_code)
        {
            case AbilityStackCode.MaxHp:
                MaxHp = jobData.maxHp + (jobData.maxHp * stackVvalue);
                break;
            case AbilityStackCode.HpRegen:
                HpRegen = jobData.hpRegen + (jobData.hpRegen * stackVvalue);
                break;
            case AbilityStackCode.AttackDamage:
                AttackDamage = jobData.attackDamage + (jobData.attackDamage * stackVvalue);
                break;
            case AbilityStackCode.AttackDelayTime:
                AttackDelayTime = jobData.attackDelayTime + -((jobData.attackDelayTime * 0.5f) * stackVvalue) * 0.5f;
                break;
            case AbilityStackCode.MovementSpeed:
                MovementSpeed = jobData.movementSpeed + (jobData.movementSpeed * stackVvalue) * 0.5f;
                animator.SetFloat("Speed", MovementSpeed / 5);
                break;
        }
    }
    [PunRPC]
    public void Resurrection()
    {
        gameObject.SetActive(true);

        hp = MaxHp;
        moveDirection = Vector3.zero;
        animator.SetFloat("Speed", MovementSpeed / 5);
    }
    public override void OnDisable()
    {
        base.OnDisable();

    }

    protected override void OnUpdate()
    {
        if (!photonView.IsMine)
            return;

        float targetDistance = GetTargetDistance();
        if (Mouse.current.leftButton.isPressed)
            weaponController.Attack(targetDistance);
        weaponController.AutoAttack(targetDistance);
    }
    protected override void OnFixedUpdate()
    {
        Vector3 playerViewMoveDirection = transform.InverseTransformDirection(moveDirection);
        if (!photonView.IsMine)
        {
            myRigidbody.position = Vector3.MoveTowards(myRigidbody.position, networkPosition, Time.fixedDeltaTime);
            myRigidbody.rotation = Quaternion.RotateTowards(myRigidbody.rotation, networkRotation, Time.fixedDeltaTime * 100.0f);

            animator.SetFloat("Horizontal", playerViewMoveDirection.x, 0.1f, Time.fixedDeltaTime);
            animator.SetFloat("Vertical", playerViewMoveDirection.z, 0.1f, Time.fixedDeltaTime);
            return;
        }

        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
        moveDirection.Set(input.x, 0, input.y);


        animator.SetFloat("Horizontal", playerViewMoveDirection.x, 0.1f, Time.fixedDeltaTime);
        animator.SetFloat("Vertical", playerViewMoveDirection.z, 0.1f, Time.fixedDeltaTime);

        Quaternion rot = Quaternion.Lerp(myRigidbody.rotation, Quaternion.LookRotation(GetTargetDirection()), turnSpeed * Time.fixedDeltaTime);
        myRigidbody.MoveRotation(rot);

        if (moveDirection != Vector3.zero)
        {
            Vector3 force = myRigidbody.position + moveDirection * MovementSpeed * Time.fixedDeltaTime;
            myRigidbody.MovePosition(force);
        }
    }
    protected override void OnDamage(float _damage, int _damageOwnerViewID) {}
    protected override void OnDeath(int _killerViewID)
    {
        PlayerManager.Instance.StartFixPlayer(this, spownPointNumber);
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        myRigidbody = GetComponent<Rigidbody>();

        myJopId = JobStorageKey.Normal;
        weaponController.InitialSetting(this);
        ChangeJob(myJopId);
        maxExperiencePoint = GetCurrentMaxExperiencePoint();

    }
    private void OnDestroy()
    {
        if (!this.gameObject.scene.isLoaded)
            return;
        PlayerManager.Instance.OutPlayerCharacter(this);
    }
    private float GetCurrentMaxExperiencePoint()
    {
        return 10f * Mathf.Pow(experienceIncreaseRate, Level);
    }
    private bool CheckAvailableForLevelUp()
    {
        if (experiencePoint >= maxExperiencePoint)
        {
            experiencePoint -= maxExperiencePoint;
            return true;
        }
        return false;
    }
    private void LevelUp()
    {
        Level++;
        AbilityStackPoint++;
        maxExperiencePoint = GetCurrentMaxExperiencePoint();
        if (jobData.promotionLevel <= Level && OnPromotionPossible != null)
            OnPromotionPossible(jobData.promotedJobs);
        if (effectOfLevelUpKey != "")
        {
            Vector3 position = transform.position + (Vector3.up * 0.5f);
            EffectPoolManager.Instance.SpawnObject(effectOfLevelUpKey, position, transform.rotation, transform);
        }
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(myRigidbody.position);
            stream.SendNext(myRigidbody.rotation);
            stream.SendNext(moveDirection);
        }
        else
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            moveDirection = (Vector3)stream.ReceiveNext();
            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));

            networkPosition += (moveDirection * MovementSpeed * lag);
        }

    }

 

}
