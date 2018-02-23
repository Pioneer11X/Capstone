using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.IO;
using System;

public class TexConverter
{
    public static Dictionary<TextureFormat, string> FormatTable { get; private set; }
    
    static TexConverter()
    {
        FormatTable = new Dictionary<TextureFormat, string>();
        FormatTable.Add(TextureFormat.Alpha8, "A8_UNORM");
        FormatTable.Add(TextureFormat.RGB24, "R8G8B8A8_UNORM");
        FormatTable.Add(TextureFormat.RGBA32, "R8G8B8A8_UNORM");
        FormatTable.Add(TextureFormat.R16, "R16_UNORM");
        FormatTable.Add(TextureFormat.DXT1, "BC1_UNORM");
        FormatTable.Add(TextureFormat.DXT5, "BC3_UNORM");
        FormatTable.Add(TextureFormat.BGRA32, "B8G8R8A8_UNORM");
        FormatTable.Add(TextureFormat.RHalf, "R16_FLOAT");
        FormatTable.Add(TextureFormat.RGHalf, "R16G16_FLOAT");
        FormatTable.Add(TextureFormat.RGBAHalf, "R16G16B16A16_FLOAT");
        FormatTable.Add(TextureFormat.RFloat, "R32_FLOAT");
        FormatTable.Add(TextureFormat.RGFloat, "R32G32_FLOAT");
        FormatTable.Add(TextureFormat.RGBAFloat, "R32G32B32A32_FLOAT");
        FormatTable.Add(TextureFormat.RGB9e5Float, "R9G9B9E5_SHAREDEXP");
        FormatTable.Add(TextureFormat.BC6H, "BC6H_UF16");
        FormatTable.Add(TextureFormat.BC7, "BC7_UNORM");
        FormatTable.Add(TextureFormat.BC4, "BC4_UNORM");
        FormatTable.Add(TextureFormat.BC5, "BC5_UNORM");
        FormatTable.Add(TextureFormat.DXT1Crunched, "BC1_UNORM");
        FormatTable.Add(TextureFormat.DXT5Crunched, "BC3_UNORM");
    }

    public static void Convert(string src, string dst, TextureFormat format, int width = 0, int height = 0, int mipmapLevels = 1, bool heightmap = false, float bumpiness = 1.0f)
    {
        string outDir = Path.GetDirectoryName(dst).Replace('/', '\\');
        //string outFilename = Path.GetFileName(dst);
        string srcBasename = Path.GetFileNameWithoutExtension(src);
        string outDDS = Path.Combine(outDir, srcBasename + ".dds");

        string args = "";
        args += " -o " + outDir;
        args += " -f " + FormatTable[format];
        args += " -m " + mipmapLevels;
        if (width > 0 && height > 0)
        {
            args += " -w " + width + " -h " + height;
        }
        if (heightmap)
        {
            args += " -nmap l -nmapamp " + bumpiness;
        }
        args += " \"" + src.Replace('/', '\\') + "\"";
        
        string configPath = Application.dataPath + "exporter.cfg";
        if (!File.Exists(configPath))
            throw new Exception("Cannot find texconv path.");
        
        FriedTofu.SceneExporter.Config config = JsonUtility.FromJson<FriedTofu.SceneExporter.Config>(File.ReadAllText(configPath));

        ProcessStartInfo info = new ProcessStartInfo(config.DXTexConvPath, args);
        info.WorkingDirectory = outDir;
        info.RedirectStandardOutput = true;
        info.RedirectStandardError = true;
        info.UseShellExecute = false;

        Process p = Process.Start(info);
        if (null == p)
            throw new Exception("Converter cannot be launched.");

        p.WaitForExit();
        UnityEngine.Debug.Log(p.StandardOutput.ReadToEnd() + "\n" +
            p.StandardError.ReadToEnd());

        if (0 != p.ExitCode)
            throw new Exception("Process failed!");

        if (!File.Exists(outDDS))
            throw new Exception("Cannot find output dds.");

        File.Copy(outDDS, dst, true);
    }

    public static void ConvertNormalMap(Texture2D normalMap, string dst)
    {
        string outDir = Path.GetDirectoryName(dst).Replace('/', '\\');
        //string outFilename = Path.GetFileName(dst);
        string srcBasename = Path.GetFileNameWithoutExtension(dst);
        string outPNG = Path.Combine(outDir, srcBasename + ".png");

        Texture2D tex = new Texture2D(normalMap.width, normalMap.height, TextureFormat.RGB24, false, false);

        for (int y = 0; y < normalMap.height; y++)
        {
            for (int x = 0; x < normalMap.width; x++)
            {
                Color c = normalMap.GetPixel(x, y);
                float r = c.a * 2 - 1;
                float g = c.g * 2 - 1;
                float b = Mathf.Sqrt(1 - r * r - g * g);
                c = new Color((r + 1) * 0.5f, (g + 1) * 0.5f, (b + 1) * 0.5f);
                tex.SetPixel(x, y, c);
            }
        }

        byte[] data = tex.EncodeToPNG();
        File.WriteAllBytes(outPNG, data);

        Convert(outPNG, dst, TextureFormat.DXT5, normalMap.width, normalMap.height, normalMap.mipmapCount);
    }
}
