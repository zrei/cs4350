public interface IHealth
{
    public void TakeDamage(float damage);

    public void Heal(float healAmount);

    // might not be needed?
    public void SetHealth(float health);
}