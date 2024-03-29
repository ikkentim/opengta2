﻿namespace OpenGta2.GameData.Scripts.Interpreting;

public enum ScriptCommandType : ushort
{
    PLAYER_PED = 0x0005,
    ARROW_DATA = 0x0017,
    GENERATOR001C = 0x001C,
    GENERATOR001D = 0x001D,
    GENERATOR001E = 0x001E,
    GENERATOR001F = 0x001F,
    GENERATOR0020 = 0x0020,
    DESTRUCTOR = 0x23,
    CONVEYOR = 0x001b,
    CRANE_DATA0026 = 0x26,
    CRANE_DATA0027 = 0x27,
    CRUSHER = 0x0028,
    OBJ_DATA0014 = 0x0014,
    OBJ_DATA0012 = 0x0012,
    RADIO_STATION = 0x011f,
    CAR_DATA = 0x0009,
    DECLARE_POWERUP_CARLIST = 0x0161,
    DECLARE_CRANE_POWERUP = 0x01b7,
    PARKED_CAR_DATA = 0x01aa,
    SOUND = 0x0147,
    OBJ_DATA0010 = 0x0010,
    LEVELSTART = 0x003b,
    LEVELEND = 0x003c,
    EXEC = 0x003f,
    CREATE_GANG_CAR018a = 0x018a,
    CREATE_GANG_CAR018b = 0x018b,
    CREATE_GANG_CAR018c = 0x018c,
    CREATE_GANG_CAR018d = 0x018d,
    SET_AMBIENT_LEVEL = 0x00e2,
    SET_SHADING_LEVEL = 0x0175,
    ENABLE_CRANE = 0x00f8,
    PUT_CAR_ON_TRAILER = 0x013c,
    GIVE_WEAPON = 0x010a,
    SET_CAR_BULLETPROOF = 0x0137,
    SET_CAR_FLAMEPROOF = 0x0139,
    SET_CAR_ROCKETPROOF = 0x0138,
    SWITCH_GENERATOR = 0x010c,
    GIVE_CAR_ALARM = 0x0136,
    ENDEXEC = 0x0040,
}
