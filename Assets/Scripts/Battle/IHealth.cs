public interface IHealth
{
    void TakeDamage(float damage);

    void Heal(float healAmount);

    // might not be needed?
    void SetHealth(float health);
}