using Palmmedia.ReportGenerator.Core.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ImageGen : MonoBehaviour
{
    [SerializeField] Renderer quad;
    Texture2D tex;
    public int width;
    public int height;
    [SerializeField] List<Color32> colours;
    void GenTexture(int w, int h)
    {
        const int alpha = 255;

        tex = new Texture2D(w, h);
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                if(colours.Count > 0)
                {
                    int max = colours.Count;
                    int index = UnityEngine.Random.Range(0, max);
                    tex.SetPixel(x, y, colours[index]);
                }
                else
                {
                    tex.SetPixel(x, y, new Color32((byte)UnityEngine.Random.Range(0, 256), (byte)UnityEngine.Random.Range(0, 256), (byte)UnityEngine.Random.Range(0, 256), alpha));
                }
            }
        }
        tex.filterMode = FilterMode.Point;
        tex.Apply();
    }

    void GenTextures()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            GenTexture(width, height);
            quad.material.SetTexture("_MainTex", tex);
        }
    }

    public void OnEndEditWidth(string widthStr)
    {
        width = Int32.Parse(widthStr);
    }

    public void OnEndEditHeight(string heightStr)
    {
        height = Int32.Parse(heightStr);
    }
}
