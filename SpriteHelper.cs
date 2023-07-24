using System.Collections.Generic;
using UnityEngine;

public class SpriteHelper
{
    public static bool Contains(List<Sprite> sprites, Sprite sprite)
    {
        if (sprites == null || sprite == null) return false;
        foreach (Sprite sp in sprites)
        {
            if (CompareSprites(sp, sprite)) return true;
        }

        return false;
    }
    
    public static bool Contains(Sprite[] sprites, Sprite sprite)
    {
        if (sprites == null || sprite == null) return false;
        foreach (Sprite sp in sprites)
        {
            if (CompareSprites(sp, sprite)) return true;
        }

        return false;
    }
    
    

    public static int IndexOf(List<Sprite> sprites, Sprite sprite)
    {
        int index = -1;
        foreach (var sp in sprites)
        {
            index++;
            if(CompareSprites(sp,sprite)) break;
        }

        return index;
    }
    
    public static int IndexOf(Sprite[] sprites, Sprite sprite)
    {
        int index = -1;
        foreach (var sp in sprites)
        {
            index++;
            if(CompareSprites(sp,sprite)) break;
        }

        return index;
    }
    
    

    public static bool CompareSprites(Sprite sprite1, Sprite sprite2)
    {
        if (!sprite1 || !sprite2) return false;
        if (sprite1.texture == sprite2.texture && sprite1.rect == sprite2.rect)
        {
            return true;
        }
        return false;
    }

}
