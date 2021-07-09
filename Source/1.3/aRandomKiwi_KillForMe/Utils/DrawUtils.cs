using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;


namespace aRandomKiwi.KFM
{
    [StaticConstructorOnStartup]
    static class DrawUtils
    {
        static DrawUtils()
        {

        }

        // Token: 0x060060BC RID: 24764 RVA: 0x002BBD68 File Offset: 0x002BA168
        public static void DrawRadiusRingEx(IntVec3 center, float radius)
        {
            ringDrawCells.Clear();
            int num = GenRadial.NumCellsInRadius(radius);
            for (int i = 0; i < num; i++)
            {
                ringDrawCells.Add(center + GenRadial.RadialPattern[i]);
            }
            DrawFieldEdgesEx(ringDrawCells);
        }

        public static void DrawFieldEdgesEx(List<IntVec3> cells)
        {
            DrawFieldEdgesEx(cells, Color.white);
        }

        public static void DrawFieldEdges(List<IntVec3> cells)
        {
            DrawFieldEdgesEx(cells, Color.white);
        }

        public static void DrawFieldEdgesEx(List<IntVec3> cells, Color color)
        {
            Map currentMap = Find.CurrentMap;
            Material material = MaterialPool.MatFrom(new MaterialRequest
            {
                shader = ShaderDatabase.Transparent,
                color = color,
                BaseTexPath = "UI/Overlays/TargetHL"
            });
            material.GetTexture("_MainTex").wrapMode = TextureWrapMode.Clamp;
            if (fieldGrid == null)
            {
                fieldGrid = new BoolGrid(currentMap);
            }
            else
            {
                fieldGrid.ClearAndResizeTo(currentMap);
            }
            int x = currentMap.Size.x;
            int z = currentMap.Size.z;
            int count = cells.Count;
            for (int i = 0; i < count; i++)
            {
                if (cells[i].InBounds(currentMap))
                {
                    fieldGrid[cells[i].x, cells[i].z] = true;
                }
            }
            for (int j = 0; j < count; j++)
            {
                IntVec3 c = cells[j];
                if (c.InBounds(currentMap))
                {
                    rotNeeded[0] = (c.z < z - 1 && !fieldGrid[c.x, c.z + 1]);
                    rotNeeded[1] = (c.x < x - 1 && !fieldGrid[c.x + 1, c.z]);
                    rotNeeded[2] = (c.z > 0 && !fieldGrid[c.x, c.z - 1]);
                    rotNeeded[3] = (c.x > 0 && !fieldGrid[c.x - 1, c.z]);
                    for (int k = 0; k < 4; k++)
                    {
                        if (rotNeeded[k])
                        {
                            Mesh plane = MeshPool.plane10;
                            Vector3 position = c.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
                            Rot4 rot = new Rot4(k);
                            Graphics.DrawMesh(plane, position, rot.AsQuat, material, 0);
                        }
                    }
                }
            }
        }


        static public void RenderMouseoverTarget()
        {
            Vector3 position = UI.MouseCell().ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
            Graphics.DrawMesh(MeshPool.plane10, position, Quaternion.identity, Utils.texAttackTarget, 0);
        }

        private static List<IntVec3> ringDrawCells = new List<IntVec3>();
        private static BoolGrid fieldGrid;
        private static bool[] rotNeeded = new bool[4];

    }
}