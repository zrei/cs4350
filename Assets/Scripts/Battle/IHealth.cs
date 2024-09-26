public interface IHealth : IStat
{
    public float CurrentHealth { get; }
    public float MaxHealth { get; }
    public bool IsDead => true;
    public void TakeDamage(float damage);

    public void Heal(float healAmount);

    // might not be needed?
    public void SetHealth(float health);

    public void Die();

    public event TrackedValueEvent OnHealthChange;
}