using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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


    public static bool CompareSprites(Sprite spriteA, Sprite spriteB)
    {
        if (spriteA == null || spriteB == null) return false;
        Texture2D textureA = DuplicateTexture(spriteA.texture);
        Texture2D textureB = DuplicateTexture(spriteB.texture);

        if (textureA.width != textureB.width || textureA.height != textureB.height)
        {
            return false; // Kích thước khác nhau
        }

        Color[] pixelsA = textureA.GetPixels();
        Color[] pixelsB = textureB.GetPixels();

        for (int i = 0; i < pixelsA.Length; i++)
        {
            if (pixelsA[i] != pixelsB[i])
            {
                return false; // Khác nhau tại pixel thứ i
            }
        }

        return true; // Trùng khớp
    }
    
    public static Texture2D DuplicateTexture(Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
            source.width,
            source.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear);

        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText;
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
