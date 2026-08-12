using System;

public interface ICharacterController
{
    public void Action();
    public event Action OnMoveSuccess;
}