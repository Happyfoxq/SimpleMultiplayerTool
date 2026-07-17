using Sandbox;
using static System.Net.Mime.MediaTypeNames;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Numerics;
using System.Linq;
public sealed class Player : Component, Component.ITriggerListener, Component.ICollisionListener
{
    [Property] public GameObject CameraPrefab;
    [Property] private Server server;
    [Property] public string PlayerName;
    [Property] public int Health;
    [Property] private int Money;
    [Property] private bool IsInvincible;
    [Property] public bool IsAlive;
    [Property] private string TeamName;
    protected override void OnStart()
    {
        base.OnStart();
        server = Scene.FindAllWithTag("Server").FirstOrDefault()?.GetComponent<Server>();
        if ( PlayerName == null || PlayerName == "" )
        {
            PlayerName = "None";
        }
        ChangeTeam( "None" );
    }
    protected override void OnUpdate()
    {

    }
    public void Teleport(Vector3 position)
    {
        GameObject.GetComponent<PlayerController>().Enabled = false;
        GameObject.WorldPosition = new Vector3( position );
        GameObject.GetComponent<PlayerController>().Enabled = true;
    }
    public void ChangeTeam(string teamname)
    {
        GameObject.Tags.Remove( TeamName );
        GameObject.Tags.Add( teamname );
        TeamName = teamname;
    }
    public void AddMoney(int amount)
    {
        if(server.DoesPlayersNeedMoney)
        {
            Money += amount;
        }
        else
        {
            Log.Error( "This sever doesn't reqire money" );
        }
    }
    public void DiscardMoney(int amount)
    {
        if(server.DoesPlayersNeedMoney)
        {
            Money -= amount;
            if (Money < 0) Money = 0;
        }
        else
        {
            Log.Error("This sever doesn't reqire money");
        }
    }
    public void TakeDamage(int amount)
    {
        if ( server.DoesPlayersNeedHealth && !IsInvincible )
        {
            Health -= amount;
            if ( Health <= 0 )
            {
                Health = 0;
                IsAlive = false;
            }
        }
        else
        {
            Log.Error("This sever doesn't reqire health or player is invincible");
        }

    }
    public void Heal(int amount)
    {
        if (!server.DoesPlayersNeedHealth)
        {
            Log.Error("This sever doesn't reqire health");
            return;
        }
        Health += amount;
    }
    public void ChangeSize( float scale )
    {
        GameObject.WorldScale = new Vector3( scale, scale, scale );
    }
    public void ResetPlayer()
    {
        GameObject.WorldScale = new Vector3( 1, 1, 1 );
        this.GameObject.GetComponent<PlayerController>().RunSpeed = 320;
        this.GameObject.GetComponent<PlayerController>().WalkSpeed = 110;
        this.GameObject.GetComponent<PlayerController>().JumpSpeed = 300;
    }
    public void ChangeNicknameColor(Color color)
    {
        var z = GetComponentInChildren<TextRenderer>();
        z.Color = color;
        Log.Info("Color changed!");
    }
    [Rpc.Owner]
    public void CreateCamera()
    {
        CameraPrefab.Clone( GameObject.WorldPosition );
    }
}