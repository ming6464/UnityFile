using System.Collections.Generic;
using UnityEngine;

public class SpriteHelper
{
    public static bool Contains(List<Sprite> sprites, Sprite sprite)
    {
        if (sprites == null || sprite == null) return false;
        foreach (var sp in sprites)
            if (CompareSprites(sp, sprite))
                return true;

        return false;
    }

    public static bool Contains(Sprite[] sprites, Sprite sprite)
    {
        if (sprites == null || sprite == null) return false;
        foreach (var sp in sprites)
            if (CompareSprites(sp, sprite))
                return true;

        return false;
    }

    public static List<int> IndexAllOf(List<Sprite> sprites, Sprite sprite)
    {
        List<int> list = new List<int>();
        for (int i = 0; i < sprites.Count; i++)
        {
            if(CompareSprites(sprites[i],sprite)) list.Add(i);
        }

        return list;
    }

    public static void RemoveAll(List<Sprite> sprites, Sprite sprite)
    {
        try
        {
            sprites.RemoveAll(x => CompareSprites(x, sprite));
        }
        catch (Exception e)
        {
            // ignored
        }
    }

    public static int IndexOf(List<Sprite> list, Sprite sprite)
    {
        var index = -1;
        if (Contains(list, sprite))
            foreach (var sp in list)
            {
                index++;
                if (CompareSprites(sp, sprite)) break;
            }

        return index;
    }

    public static int IndexOf(Sprite[] sprites, Sprite sprite)
    {
        var index = -1;
        foreach (var sp in sprites)
        {
            index++;
            if (CompareSprites(sp, sprite)) break;
        }

        return index;
    }


    public static bool CompareSprites(Sprite sprite1, Sprite sprite2)
    {
        if (!sprite1 || !sprite2) return false;
        if (sprite1.texture == sprite2.texture && sprite1.rect == sprite2.rect) return true;
        return false;
    }
    
    public static void ChangeSkin(SpriteRenderer partImage, Sprite partSprite)
    {
        if (!partImage) return;
        partImage.sprite = partSprite;
        partImage.enabled = true;
    }

    public static void ChangeSkin(Image partImage, Sprite partSprite)
    {
        if (!partImage) return;
        partImage.sprite = partSprite;
        partImage.enabled = true;
    }
}
