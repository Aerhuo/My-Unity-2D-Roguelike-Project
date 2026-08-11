using System;

public interface IBrain
{
    public bool IsThinking { get; }
    public void ExcuteLogic();
}