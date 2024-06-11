using Avalonia.Input;
using Digger.Architecture;
using System;

namespace Digger;

public class Terrain : ICreature
{
    public CreatureCommand Act(int x, int y)
    {
        return new CreatureCommand
        {
            DeltaX = 0,
            DeltaY = 0
        };
    }

    public bool DeadInConflict(ICreature conflictedObject)
    {
        return true;
    }

    public int GetDrawingPriority()
    {
        return 5;
    }

    public string GetImageFileName()
    {
        return "Terrain.png";
    }
}

public class Player : ICreature
{
    public CreatureCommand Act(int x, int y)
    {
        var dX = 0;
        var dY = 0;
        var key = Game.KeyPressed;
        switch (key)
        {
            case Key.Left:
                if (x != 0 && Game.Map[x - 1, y] is not Sack)
                    dX = -1;
                break;
            case Key.Up:
                if (y != 0 && Game.Map[x, y - 1] is not Sack)
                    dY = -1;
                break;
            case Key.Right:
                if (x != Game.MapWidth - 1 && Game.Map[x + 1, y] is not Sack)
                    dX = 1;
                break;
            case Key.Down:
                if (y != Game.MapHeight - 1 && Game.Map[x, y + 1] is not Sack)
                    dY = 1;
                break;
        }
        return new CreatureCommand
        {
            DeltaX = dX,
            DeltaY = dY
        };
    }

    public bool DeadInConflict(ICreature conflictedObject)
    {
        return conflictedObject is Sack || conflictedObject is Monster;
    }

    public int GetDrawingPriority()
    {
        return 2;
    }

    public string GetImageFileName()
    {
        return "Digger.png";
    }
}

public class Sack : ICreature
{
    int FallingCounter = 0;
    public CreatureCommand Act(int x, int y)
    {
        ICreature tr = this;

        var dY = 0;
        if (y + 1 != Game.MapHeight && (Game.Map[x, y + 1] == null
            || ((Game.Map[x, y + 1] is Player) || (Game.Map[x, y + 1] is Monster))
            && FallingCounter > 0))
        {
            dY = 1;
            FallingCounter++;
        }
        else
        {
            if (FallingCounter > 1)
            {
                tr = new Gold();
            }
            FallingCounter = 0;
        }


        return new CreatureCommand
        {
            DeltaX = 0,
            DeltaY = dY,
            TransformTo = tr
        };
    }

    public bool DeadInConflict(ICreature conflictedObject)
    {
        return false;
    }

    public int GetDrawingPriority()
    {
        return 0;
    }

    public string GetImageFileName()
    {
        return "Sack.png";
    }
}

public class Gold : ICreature
{
    public CreatureCommand Act(int x, int y)
    {
        return new CreatureCommand
        {
            DeltaX = 0,
            DeltaY = 0
        };
    }

    public bool DeadInConflict(ICreature conflictedObject)
    {
        if (conflictedObject is Player)
        {
            Game.Scores += 10;
            return true;
        }
        return conflictedObject is Monster;
    }

    public int GetDrawingPriority()
    {
        return 4;
    }

    public string GetImageFileName()
    {
        return "Gold.png";
    }
}


public class Monster : ICreature
{
    private static bool CorrectMove(int x, int y, int dX, int dY)
    {
        return Game.Map[x + dX, y + dY] is not Terrain
            && Game.Map[x + dX, y + dY] is not Sack
            && Game.Map[x + dX, y + dY] is not Monster;
    }

    private static int[] FindPlayer()
    {
        for (int y = 0; y < Game.Map.GetLength(1); y++)
            for (int x = 0; x < Game.Map.GetLength(0); x++)
            {
                if ((Game.Map[x, y] is Player))
                {
                    var result = new int[2];
                    result[0] = x;
                    result[1] = y;
                    return result;
                }
            }
        return null;
    }

    private static int[] MonsterMove(int x, int y, int[] player)
    {
        var dX = 0;
        var dY = 0;
        var xDist = player[0] - x;
        var yDist = player[1] - y;

        var xMove = Math.Sign(xDist);
        var yMove = Math.Sign(yDist);

        if (Math.Abs(xDist) >= Math.Abs(yDist))
        {
            if (CorrectMove(x, y, xMove, 0))
                dX = xMove;
            else if (CorrectMove(x, y, 0, yMove))
                dY = yMove;
        }
        else
        {
            if (CorrectMove(x, y, 0, yMove))
                dY = yMove;
            else if (CorrectMove(x, y, xMove, 0))
                dX = xMove;
        }
        return new int[] { dX, dY };
    }

    public CreatureCommand Act(int x, int y)
    {
        var player = FindPlayer();
        if (player == null)
            return new CreatureCommand
            {
                DeltaX = 0,
                DeltaY = 0
            };

        var move = MonsterMove(x, y, player);
        return new CreatureCommand
        {
            DeltaX = move[0],
            DeltaY = move[1]
        };
    }

    public bool DeadInConflict(ICreature conflictedObject)
    {
        return conflictedObject is Sack || conflictedObject is Monster;
    }

    public int GetDrawingPriority()
    {
        return 1;
    }

    public string GetImageFileName()
    {
        return "Monster.png";
    }
}
