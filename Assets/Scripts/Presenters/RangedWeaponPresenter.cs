using Unity.Netcode;
using UnityEngine;

public class RangedWeaponPresenter : WeaponPresenter
{
    [SerializeField] private Player player;
    [SerializeField] private AmmoMag model;
    [SerializeField] private WeaponViewUI view;
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform origin;
    
    public void Restore()
    {
        model?.Restore();
    }
    
    public void AddAmmo(int amount)
    {
        if (!model)
        {
            return;
        }
        
        model.AddAmmo(amount);
    }
    
    public override void UseWeapon(StatePayload callingState)
    {
        if (!model || !player || Time.time <= NextUseCooldownTime)
        {
            return;
        }

        var wasAmmoRemoved = model.RemoveAmmo(1);

        if (!wasAmmoRemoved)
        {
            return;
        }

        NextUseCooldownTime = Time.time + cooldown;

        //as the owner we just instantiate locally at our shoot position
        if (IsOwner)
        {
            LocalShoot();

            //as a server we also call the instantiation rpc
            if (IsServer)
                ProjectileInstantiationRpc(origin.position, origin.rotation);

            return;
        }

        //if we're not the owner and not the server we don't do anything
        if (!IsServer)
            return;

        //if we're the server we perform the lag compensated projectile instantiation
        LagCompensatedShoot(callingState.SnapshotTick);
    }

    private Projectile InstantiateProjectileAt(Vector3 position, Quaternion rotation)
    {
        // we can just instantiate as a class reference to avoid unnecessary GetComponent calls
        var projectile = Instantiate(projectilePrefab, position, rotation);
        // assign the source of the object to handle kill counters and such
        projectile.SetDamageMultiplier(damageMultiplier);
        projectile.SetSource(player);
        projectile.SetAttackStartPosition(player.transform.position);

        return projectile;
    }
    private void LocalShoot()
    {
        Debug.Log("Local shoot");
        InstantiateProjectileAt(origin.position, origin.rotation);
    }

    private void LagCompensatedShoot(int snapshotTick)
    {
        Debug.Log("Lag compensated shoot");
        var projectile = InstantiateProjectileAt(origin.position, origin.rotation);

        float interval = NetworkManager.ServerTime.Tick - snapshotTick;
        Vector3 distanceTravelledDuringInterval = projectile.transform.right * projectile.Speed * interval * (1f / NetworkManager.NetworkTickSystem.TickRate);
        Vector3 correctedPosition = projectile.transform.position + distanceTravelledDuringInterval;
        
        //debugging
        Debug.DrawLine(origin.position, origin.position + distanceTravelledDuringInterval, Color.blue, 5f);

        //put projectile into ignore raycast layer and check for rewind period hit
        int originalProjectileLayer = projectile.gameObject.layer;
        projectile.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        Debug.Log(projectile.gameObject.layer);
        Transform rewindPeriodHit = GlobalStateManager.CheckForRewindPeriodHit(snapshotTick, origin.position, distanceTravelledDuringInterval);
        //put it back into its original layer
        projectile.gameObject.layer = originalProjectileLayer;

        //if we hit someone in the rewind period then immediately call the impact on the projectile
        if (rewindPeriodHit != null)
        {
            projectile.CollidedWith(rewindPeriodHit);
            return;
        }

        //else reposition the projectile to the corrected position, call an rpc for the other clients to instantiate a projectile from there
        projectile.transform.position = correctedPosition;
        ProjectileInstantiationRpc(correctedPosition, origin.rotation);
    }

    [Rpc(SendTo.NotServer)]
    public void ProjectileInstantiationRpc(Vector3 position, Quaternion rotation)
    {
        //only instantiate on remote clients
        if (IsOwner)
            return;

        InstantiateProjectileAt(position, rotation);
    }

    public override void Equip()
    {
    }
    
    public override void Dequip()
    {
    }
    
    private void Start()
    {
        if (model)
        {
            model.OnAmmoChanged += AmmoChanged;
        }
        
        UpdateView();
    }
    
    public override void OnDestroy()
    {
        if (model)
        {
            model.OnAmmoChanged -= AmmoChanged;
        }
        base.OnDestroy();
    }
    
    private void AmmoChanged()
    {
        UpdateView();
    }
    
    private void UpdateView()
    {
        if (!model || !view)
        {
            return;
        }
        
        view.UpdateAmmoTextValue(model.CurrentAmmo, model.MaxAmmo);
    }
}