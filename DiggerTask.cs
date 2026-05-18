using Avalonia.Input;
using Digger.Architecture;
using System;
using System.Threading;
using static Digger.Terrain;

namespace Digger;

//Напишите здесь классы Player, Terrain и другие.

public class Player : ICreature
{
    string ICreature.GetImageFileName()
    {
        return "Digger.png";
    }

    int ICreature.GetDrawingPriority()
    {
        return 10;
    }

    public CreatureCommand Act(int x, int y)
    {
        switch (Game.KeyPressed)
        {
            case Key.Right:
                if ((x + 1 < Game.MapWidth) && !(Game.Map[x + 1, y] is Sack))
                {
                    return new CreatureCommand { DeltaX = 1, DeltaY = 0 };
                }
                return new CreatureCommand { DeltaX = 0, DeltaY = 0 };

            case Key.Left:
                if ((x - 1 >= 0) && !(Game.Map[x - 1, y] is Sack))
                {
                    return new CreatureCommand { DeltaX = -1, DeltaY = 0 };
                }
                return new CreatureCommand { DeltaX = 0, DeltaY = 0 };

            case Key.Up:
                if ((y - 1 >= 0) && !(Game.Map[x, y - 1] is Sack))
                {
                    return new CreatureCommand { DeltaX = 0, DeltaY = -1 };
                }
                return new CreatureCommand { DeltaX = 0, DeltaY = 0 };

            case Key.Down:
                if ((y + 1 < Game.MapHeight) && !(Game.Map[x, y + 1] is Sack))
                {
                    return new CreatureCommand { DeltaX = 0, DeltaY = 1 };
                }
                return new CreatureCommand { DeltaX = 0, DeltaY = 0 };

            default:
                return new CreatureCommand { DeltaX = 0, DeltaY = 0 };
        }
    }

    bool ICreature.DeadInConflict(ICreature conflictedObject)
    {
        return (conflictedObject is Sack) || (conflictedObject is Monster);
    }
}

internal class Terrain : ICreature
{
    string ICreature.GetImageFileName()
    {
        return "Terrain.png";
    }

    int ICreature.GetDrawingPriority()
    {
        return 5;
    }

    public CreatureCommand Act(int x, int y)
    {
        return new CreatureCommand { DeltaX = 0, DeltaY = 0 };
    }

    public bool DeadInConflict(ICreature conflictedObject)
    {
        if (conflictedObject is Player)
        {
            return true;
        }
        return false;
    }
}

public class Sack : ICreature
{
    private int DropCounter = 0;

    string ICreature.GetImageFileName()
    {
        return "Sack.png";
    }

    int ICreature.GetDrawingPriority()
    {
        return 15;
    }

    public CreatureCommand Act(int x, int y)
    {
        CreatureCommand command = new CreatureCommand();
        if (y + 1 < Game.MapHeight && (Game.Map[x, y + 1] is null || 
            (DropCounter > 0 && Game.Map[x, y + 1] is Player) 
            || (DropCounter > 0 && Game.Map[x, y + 1] is Monster)))
        {
            DropCounter++;
            return new CreatureCommand { DeltaX = 0, DeltaY = 1 };
        }
        if (DropCounter > 1)
        {
            return new CreatureCommand { DeltaX = 0, DeltaY = 0, TransformTo = new Gold() };
        }
        DropCounter = 0;
        return new CreatureCommand { DeltaX = 0, DeltaY = 0 };
    }

    bool ICreature.DeadInConflict(ICreature conflictedObject)
    {
        return false;
    }
}

public class Gold : ICreature
{
    string ICreature.GetImageFileName()
    {
        return "Gold.png";
    }

    int ICreature.GetDrawingPriority()
    {
        return 15;
    }

    bool ICreature.DeadInConflict(ICreature conflictedObject)
    {
        if (conflictedObject is Player)
        {
            Game.Scores += 10;
            return true;
        }
        else if (conflictedObject is Monster)
        {
            return true;
        }
        return false;
    }

    public CreatureCommand Act(int x, int y)
    {
        return new CreatureCommand { DeltaX = 0, DeltaY = 0 };
    }
}

public class Monster : ICreature
{
    string ICreature.GetImageFileName()
    {
        return "Monster.png";
    }

    int ICreature.GetDrawingPriority()
    {
        return 10;
    }

    bool ICreature.DeadInConflict(ICreature conflictedObject)
    {
        if (conflictedObject is Sack || conflictedObject is Monster)
        {
            return true;
        }
        return false;
    }

    public CreatureCommand Act(int x, int y)
    {
        var (stepX, stepY) = FindPlayer(x, y);
        if (stepX != 0 || stepY != 0)
        {
            if ((stepX == 1) && (x + 1 < Game.MapWidth) && !(Game.Map[x + 1, y] is Terrain) 
                && !(Game.Map[x + 1, y] is Sack) && !(Game.Map[x + 1, y] is Monster))
            {
                return new CreatureCommand { DeltaX = 1, DeltaY = 0 };
            }
            else if ((stepX == -1) && (x - 1 >= 0) && !(Game.Map[x - 1, y] is Terrain) 
                && !(Game.Map[x - 1, y] is Sack) && !(Game.Map[x - 1, y] is Monster))
            {
                return new CreatureCommand { DeltaX = -1, DeltaY = 0 };
            }
            else
            {
                if ((stepY == 1) && (y + 1 < Game.MapHeight) && !(Game.Map[x, y + 1] is Terrain) 
                    && !(Game.Map[x, y + 1] is Sack) && !(Game.Map[x, y + 1] is Monster))
                {
                    return new CreatureCommand { DeltaX = 0, DeltaY = 1 };
                }
                if ((stepY == -1) &&(y - 1 >= 0) && !(Game.Map[x, y - 1] is Terrain) && 
                    !(Game.Map[x, y - 1] is Sack) && !(Game.Map[x, y - 1] is Monster))
                {
                    return new CreatureCommand { DeltaX = 0, DeltaY = -1 };
                }
            }
        }
        return new CreatureCommand { DeltaX = 0, DeltaY = 0 };
    }
        

    

    private static (int, int) FindPlayer(int x, int y)
    {
        for (int i = 0; i < Game.Map.GetLength(0); i++)
        {
            for (int j = 0; j < Game.Map.GetLength(1); j++)
            {
                if ((Game.Map[i, j] is Player))
                {
                    return (Math.Sign(i - x), Math.Sign(j - y));
                }
            }
        }
        return (0, 0);
    }
}