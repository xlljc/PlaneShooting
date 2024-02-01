﻿using Godot;

/// <summary>
/// 宝箱
/// </summary>
[Tool]
public partial class TreasureBox : ActivityObject, IHurt
{
    public bool IsOpen { get; private set; }

    public override void OnInit()
    {
        AnimatedSprite.AnimationFinished += OnAnimationFinished;
    }

    public override CheckInteractiveResult CheckInteractive(ActivityObject master)
    {
        return new CheckInteractiveResult(this, !IsOpen, CheckInteractiveResult.InteractiveType.OpenTreasureBox);
    }

    public override void Interactive(ActivityObject master)
    {
        if (IsOpen)
        {
            return;
        }

        IsOpen = true;
        AnimatedSprite.Play(AnimatorNames.Open);
    }

    private void OnAnimationFinished()
    {
        var weapon = Create(World.RandomPool.GetRandomProp());
        weapon.Throw(Position, 2, 90, new Vector2(0, 13), 0);
    }

    public void Hurt(ActivityObject target, int damage, float angle)
    {
        PlayHitAnimation();
        Debug.Log("111");
    }
}