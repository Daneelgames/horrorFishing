using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSkills", menuName = "Skills Data")]
public class SkillsData : ScriptableObject
{
    public enum Skill
    {
        RunCostLower,
        DashCostLower,
        RunSpeedBoost,
        ResourceMagneto,
        FireAttack,
        RandomTeleport,
        Hallucinations,
        Vertigo,
        HpUp,
        DashAttack,
        FastStraifing,
        ShootOnRun,
        StrongWeapon,
        HealthForGold,
        AmmoForHealth,
        MirrorDamage,
        GoldShooter,
        ThirdArm,
        Hunk,
        PipeLover,
        KnifeLover,
        AxeLover,
        PistolLover,
        RevolverLover,
        ShotgunLover,
        TommyLover,
        DashOnAttack,
        ThirtyFingers,
        NoseWithTeeth,
        TrapLover,
        Handsome,
        SoftFeet,
        SteelSpine,
        DickheadStomach,
        HorrorMode,
        SlowMoDash,
        WallEater,
        Traitor
    }

    public List<SkillInfo> skills;
}

[Serializable]
public class SkillInfo
{
    public int skillIndex = 0;
    public SkillsData.Skill skill;
    public Sprite image;
    public string info = "";
    public string infoRu = "";
    public string infoESP = "";
    public string infoGER = "";
    public string infoIT = "";
    public string infoSPBR = "";

    public bool coopOnly = false;
    public bool soloOnly = false;
}