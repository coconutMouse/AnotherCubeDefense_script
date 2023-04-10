
public class MainTowerCharacter : Character
{
    public GameSystem gameSystem;

    public override void OnEnable()
    {
        base.OnEnable();
        InitialSetting();
    }
    public void InitialSetting()
    {
        hp = MaxHp;
    }

    protected override void OnUpdate() {}
    protected override void OnFixedUpdate() {}
    protected override void OnDamage(float _damage, int _damageOwnerViewID) {}

    protected override void OnDeath(int _killerViewID)
    {
        PlayerManager.Instance.StopFixPlayerAll();
        CharacterManager.Instance.StopAllCharacterUpdates();
        gameSystem.GameOver();
    }
}
