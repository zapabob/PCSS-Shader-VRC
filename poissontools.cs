// PoissonTools.cs
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PoissonTools : MonoBehaviour
{
    #region Data

    //128 Samples
    public Vector2[] data = new Vector2[]
    {
        new Vector2 (-0.1751133f, -0.7623146f),
        new Vector2 (-0.02656096f, -0.6266535f),
        new Vector2 (-0.2567535f, -0.6382824f),
        new Vector2 (0.03169665f, -0.7597309f),
        new Vector2 (-0.08184267f, -0.854284f),
        new Vector2 (-0.249699f, -0.8949153f),
        new Vector2 (-0.3884096f, -0.6610415f),
        new Vector2 (-0.4017575f, -0.8185323f),
        new Vector2 (0.143545f, -0.8252903f),
        new Vector2 (-0.0004691426f, -0.9640467f),
        new Vector2 (0.2801128f, -0.7629396f),
        new Vector2 (-0.530535f, -0.7206488f),
        new Vector2 (-0.7046922f, -0.6934857f),
        new Vector2 (-0.5364909f, -0.5190643f),
        new Vector2 (-0.1572677f, -0.5521687f),
        new Vector2 (-0.1930406f, -0.4002087f),
        new Vector2 (-0.4024554f, -0.4846089f),
        new Vector2 (0.2255861f, -0.9429856f),
        new Vector2 (0.4244879f, -0.8863587f),
        new Vector2 (0.4560401f, -0.695415f),
        new Vector2 (0.3627229f, -0.5412803f),
        new Vector2 (0.156915f, -0.6732631f),
        new Vector2 (0.2539547f, -0.4027954f),
        new Vector2 (0.4992049f, -0.569185f),
        new Vector2 (0.5069118f, -0.4363518f),
        new Vector2 (0.1478627f, -0.498681f),
        new Vector2 (-0.7317886f, -0.5510861f),
        new Vector2 (0.7273099f, -0.5381364f),
        new Vector2 (0.6334916f, -0.3718306f),
        new Vector2 (0.630892f, -0.6550488f),
        new Vector2 (0.005001811f, -0.5014819f),
        new Vector2 (0.1088556f, -0.3350014f),
        new Vector2 (-0.09200726f, -0.273784f),
        new Vector2 (0.2228594f, -0.2722488f),
        new Vector2 (0.05839515f, -0.1157391f),
        new Vector2 (0.3097804f, -0.113203f),
        new Vector2 (0.3968651f, -0.2324289f),
        new Vector2 (0.1925638f, -0.03927073f),
        new Vector2 (0.3478966f, 0.1147794f),
        new Vector2 (0.048101f, 0.1718512f),
        new Vector2 (0.2037343f, 0.1791027f),
        new Vector2 (-0.2278407f, -0.2698237f),
        new Vector2 (-0.2293893f, -0.08886856f),
        new Vector2 (-0.0724064f, -0.1103706f),
        new Vector2 (-0.3608528f, -0.3093037f),
        new Vector2 (-0.5018233f, -0.2594209f),
        new Vector2 (-0.6152774f, -0.3919068f),
        new Vector2 (0.5458633f, -0.832506f),
        new Vector2 (0.8959236f, -0.4177609f),
        new Vector2 (0.7552829f, -0.1990608f),
        new Vector2 (0.5703962f, -0.1580789f),
        new Vector2 (0.8125386f, -0.3183063f),
        new Vector2 (0.4326977f, -0.02849672f),
        new Vector2 (-0.7578087f, -0.3455803f),
        new Vector2 (-0.8781314f, -0.4547133f),
        new Vector2 (-0.7587945f, -0.1111522f),
        new Vector2 (-0.6758134f, -0.2383514f),
        new Vector2 (-0.9335123f, -0.1905898f),
        new Vector2 (-0.2405534f, 0.1575762f),
        new Vector2 (-0.3478258f, 0.01080485f),
        new Vector2 (-0.3946321f, -0.1553284f),
        new Vector2 (-0.121407f, 0.01730447f),
        new Vector2 (0.008169299f, 0.00237719f),
        new Vector2 (0.7531912f, -0.05407249f),
        new Vector2 (0.6107084f, 0.07557391f),
        new Vector2 (-0.5426206f, -0.04421606f),
        new Vector2 (0.9434871f, -0.2427984f),
        new Vector2 (0.79834f, 0.2003977f),
        new Vector2 (0.6386936f, 0.2067907f),
        new Vector2 (0.5162001f, 0.2481175f),
        new Vector2 (0.8486845f, 0.05016677f),
        new Vector2 (0.1310353f, 0.3870411f),
        new Vector2 (0.3443319f, 0.2591961f),
        new Vector2 (0.9460863f, -0.0999916f),
        new Vector2 (0.5197876f, -0.2776032f),
        new Vector2 (-0.655161f, 0.0813053f),
        new Vector2 (-0.5192261f, 0.1622644f),
        new Vector2 (-0.1034693f, 0.1961033f),
        new Vector2 (-0.01287371f, 0.4132029f),
        new Vector2 (-0.2325477f, 0.4170833f),
        new Vector2 (-0.4455887f, 0.3052129f),
        new Vector2 (-0.1941404f, 0.5715412f),
        new Vector2 (-0.3819574f, 0.5124161f),
        new Vector2 (0.939623f, 0.2110529f),
        new Vector2 (0.7747282f, 0.3351696f),
        new Vector2 (0.916173f, 0.3637665f),
        new Vector2 (0.6122758f, 0.3726794f),
        new Vector2 (0.4529108f, 0.4910271f),
        new Vector2 (0.3121231f, 0.4577765f),
        new Vector2 (-0.1231295f, 0.3382495f),
        new Vector2 (0.1325305f, 0.6223459f),
        new Vector2 (0.2647518f, 0.6570308f),
        new Vector2 (0.4434021f, 0.3624335f),
        new Vector2 (0.01515486f, 0.5425494f),
        new Vector2 (0.3968382f, 0.6275262f),
        new Vector2 (-0.555426f, 0.497408f),
        new Vector2 (-0.4766474f, 0.6577536f),
        new Vector2 (-0.3061574f, 0.2923721f),
        new Vector2 (0.6069424f, 0.5025209f),
        new Vector2 (0.5482251f, 0.7110948f),
        new Vector2 (0.3091147f, 0.8383397f),
        new Vector2 (0.47799f, 0.8276356f),
        new Vector2 (0.8143722f, 0.5142422f),
        new Vector2 (0.7506019f, 0.641673f),
        new Vector2 (-0.9087288f, 0.02085561f),
        new Vector2 (-0.8099157f, 0.1042998f),
        new Vector2 (-0.7361311f, 0.2937286f),
        new Vector2 (-0.5891773f, 0.2790734f),
        new Vector2 (0.1710244f, 0.7607511f),
        new Vector2 (0.03757187f, 0.7587031f),
        new Vector2 (-0.2531807f, 0.6851944f),
        new Vector2 (-0.7373943f, 0.4311338f),
        new Vector2 (-0.8966049f, -0.3273084f),
        new Vector2 (-0.9556297f, 0.1463697f),
        new Vector2 (-0.1005473f, 0.7755122f),
        new Vector2 (-0.8917782f, 0.3278767f),
        new Vector2 (-0.2283025f, 0.8528607f),
        new Vector2 (-0.3668827f, 0.8998415f),
        new Vector2 (-0.4454894f, 0.7887819f),
        new Vector2 (-0.01854463f, 0.9130806f),
        new Vector2 (-0.1936664f, 0.9784864f),
        new Vector2 (-0.3925535f, 0.1408866f),
        new Vector2 (-0.8808329f, 0.4699932f),
        new Vector2 (-0.7065572f, 0.5662754f),
        new Vector2 (-0.6635622f, 0.6873093f),
        new Vector2 (0.1050287f, 0.870977f),
        new Vector2 (-0.04707854f, 0.6568419f),
        new Vector2 (0.1912594f, 0.9769967f)
    };

    [HideInInspector]
    public Vector2[] dataBackup;

    #endregion

    #region Variables

    [Header("Misc Settings")]
    public int interleaveInterval = 2;
    public int seed = 42;

    [Space(5f)]
    [Header("IntelliSort Settings")]
    public AnimationCurve radialWeight;

    [Space(5f)]
    [Header("Debug Display Output")]
    public int res = 512;
    public Texture2D tex;
    public Material mat;

    [Range(1, 128)]
    public int count = 64;


    #endregion

    #region Data Manipulations

    [ContextMenu("Interleave")]
    public void Interleave ()
    {
        Backup();

        int midPoint = Mathf.CeilToInt(data.Length / 2f);
        for (int i = 1; i < midPoint; i += interleaveInterval)
        {
            int a = i;
            int b = data.Length - 1 - i;

            Vector2 aVal = data[a];
            Vector2 bVal = data[b];

            data[a] = bVal;
            data[b] = aVal;
        }
    }

    [ContextMenu("Shuffle")]
    public void ShuffleData ()
    {
        Backup();

        int r;
        Vector2 tmp;
        for (int i = data.Length - 1; i > 0; i--)
        {
            r = Random.Range(0, i);
            tmp = data[i];
            data[i] = data[r];
            data[r] = tmp;
        }
    }

    [ContextMenu("IntelliSort")]
    public void IntelliSort ()
    {
        Backup();

        var newData = new List<Vector2>(data.Length);
        var remainingSamples = new List<Vector2>(data.Length);
        remainingSamples.AddRange(data);

        var directions = new Vector2[] { Vector2.zero, Vector2.one, -Vector2.one, new Vector2(-1, 1), new Vector2(1, -1), Vector2.left, Vector2.right, Vector2.down, Vector2.up };

        for (int i = 0; i < directions.Length; i++)
        {
            remainingSamples = remainingSamples.OrderBy(s => (s - directions[i]).sqrMagnitude).ToList();
            newData.Add(remainingSamples[0]);
            remainingSamples.RemoveAt(0);
        }

        while (remainingSamples.Count > 0)
        {
            float weight = radialWeight.Evaluate(newData.Count / (data.Length - 1f));
            float maxDist = 0f;
            int furthestSample = 0;

            for (int rsIndex = 0; rsIndex < remainingSamples.Count; rsIndex++)
            {
                var rs = remainingSamples[rsIndex];
                rs = rs.normalized * Mathf.Pow(rs.magnitude, weight);

                float minDist = Mathf.Infinity;

                for (int ndIndex = 0; ndIndex < newData.Count; ndIndex++)
                {
                    var dist = (rs - newData[ndIndex]).sqrMagnitude;
                    if (dist < minDist)
                    {
                        minDist = dist;
                    }
                }

                if (minDist > maxDist)
                {
                    maxDist = minDist;
                    furthestSample = rsIndex;
                }
            }

            newData.Add(remainingSamples[furthestSample]);
            remainingSamples.RemoveAt(furthestSample);
        }

        data = newData.ToArray();
    }

    [ContextMenu("Sort By X")]
    public void SortByX ()
    {
        Backup();

        data = data.OrderBy(v => v.x).ToArray();
    }

    #endregion

    #region Output Helpers

    [ContextMenu("Redraw")]
    public void Redraw ()
    {
        Random.InitState(seed);
        tex = new Texture2D(res, res);
        Color[] pixels = new Color[res * res];

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.black;
        }

        for (int i = 0; i < count; i++)
        {
            Vector2 remappedData = (data[i] + Vector2.one) * .5f * (res - 1);

            int x = Mathf.RoundToInt(remappedData.x);
            int y = Mathf.RoundToInt(remappedData.y);

            int index = y * res + x;
            pixels[index] = Color.white;

            if (x > 0)
            {
                pixels[index - 1] = Color.gray;
            }
            if (x < res - 1)
            {
                pixels[index + 1] = Color.gray;
            }

            if (y > 0)
            {
                pixels[index - res] = Color.gray;
            }
            if (y < res - 1)
            {
                pixels[index + res] = Color.gray;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        if (mat != null)
            mat.SetTexture("_MainTex", tex);
    }

    [ContextMenu("Print Data")]
    public void PrintArray ()
    {
        string hlslOutput = "static const float2 PoissonOffsets[" + data.Length + "] = {\n";
        string csOutput = "Vector2 PoissonOffsets = new Vector2[]\n{\n";
        for (int i = 0; i < data.Length; i++)
        {
            hlslOutput += string.Format("\tfloat2({0}, {1}),\n", data[i].x, data[i].y);
            csOutput += string.Format("\tnew Vector2({0}f, {1}f),\n", data[i].x, data[i].y);
        }
        hlslOutput += "};";
        csOutput += "};";
        Debug.Log(hlslOutput);
        Debug.Log(csOutput);
    }

    #endregion

    #region Backup

    [ContextMenu("Revert To Backup")]
    public void Revert ()
    {
        dataBackup.CopyTo(data, 0);
    }

    public void Backup (bool forced = false)
    {
        if (forced || dataBackup == null || dataBackup.Length == 0)
        {
            dataBackup = new Vector2[data.Length];
            data.CopyTo(dataBackup, 0);
        }
    }

    #endregion
}