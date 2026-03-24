using UnityEngine;

public static class GameConfig
{
    public enum ControlMode
    {
        Human,
        AI
    }

    public static ControlMode RunnerMode = ControlMode.AI;
    public static ControlMode PursuerMode = ControlMode.AI;
}
