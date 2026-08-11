using System;

public interface ITurnTaker
{
    public bool NeedWait { get; }
    public bool IsActing { get; }
    public bool CanAct { get; }
    public bool Top { get; }
    public void TakeTurn();
    public void RecoveryEnergy();
    public void ConsumeEnergy();
}