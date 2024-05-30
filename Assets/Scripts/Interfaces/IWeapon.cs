public interface IWeapon
{
    void UseWeapon(StatePayload callingState);
    void Equip();
    void Dequip();
}