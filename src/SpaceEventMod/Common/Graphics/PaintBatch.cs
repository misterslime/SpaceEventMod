using Daybreak.Common.Features.Hooks;
using Daybreak.Common.Features.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace SpaceEventMod.Common.Graphics;

internal class PaintBatch(GraphicsDevice graphicsDevice) : SpriteBatch(graphicsDevice)
{
    private sealed class Instance : IStatic<Instance>
    {
        public required PaintBatch PaintBatchInstance { get; init; }

        public static Instance LoadData(Mod mod)
        {
            return Main.RunOnMainThread(
                () => new Instance
                {
                    PaintBatchInstance = new PaintBatch(Main.graphics.GraphicsDevice)
                }
            ).GetAwaiter().GetResult();
        }

        public static void UnloadData(Instance data)
        {
            data.PaintBatchInstance.Dispose();
        }
    }

    public static PaintBatch instance => Instance.Instance.PaintBatchInstance;

    private int _pass;

    public void Begin(SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState, Effect effect, Matrix transformMatrix, int pass = 0)
    {
        base.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, transformMatrix);

        this._pass = pass;
    }

    #region Modified SpriteBatch Methods
    public new void End()
    {
        if (!beginCalled)
        {
            throw new InvalidOperationException("End was called, but Begin has not yet been called. You must call Begin  successfully before you can call End.");
        }

        beginCalled = false;
        if (sortMode != SpriteSortMode.Immediate)
        {
            FlushBatch();
        }

        customEffect = null;
    }

    public new unsafe void FlushBatch()
    {
        PrepRenderState();
        if (numSprites == 0)
        {
            return;
        }

        if (sortMode != SpriteSortMode.Deferred)
        {
            IComparer<nint> comparer = ((sortMode == SpriteSortMode.Texture) ? TextureCompare : ((sortMode != SpriteSortMode.BackToFront) ? ((IComparer<nint>)FrontToBackCompare) : ((IComparer<nint>)BackToFrontCompare)));
            fixed (SpriteInfo* ptr = &spriteInfos[0])
            {
                fixed (nint* ptr2 = &sortedSpriteInfos[0])
                {
                    fixed (VertexPositionColorTexture4* ptr3 = &vertexInfo[0])
                    {
                        for (int i = 0; i < numSprites; i++)
                        {
                            ptr2[i] = (nint)(ptr + i);
                        }

                        Array.Sort(sortedSpriteInfos, textureInfo, 0, numSprites, comparer);
                        for (int j = 0; j < numSprites; j++)
                        {
                            SpriteInfo* ptr4 = (SpriteInfo*)ptr2[j];
                            GenerateVertexInfo(ptr3 + j, ptr4->sourceX, ptr4->sourceY, ptr4->sourceW, ptr4->sourceH, ptr4->destinationX, ptr4->destinationY, ptr4->destinationW, ptr4->destinationH, ptr4->color, ptr4->originX, ptr4->originY, ptr4->rotationSin, ptr4->rotationCos, ptr4->depth, ptr4->effects);
                        }
                    }
                }
            }
        }

        int num = 0;
        while (true)
        {
            int num2 = Math.Min(numSprites, 2048);
            int num3 = UpdateVertexBuffer(num, num2);
            int num4 = 0;
            Texture2D texture2D = textureInfo[num];
            for (int k = 1; k < num2; k++)
            {
                Texture2D texture2D2 = textureInfo[num + k];
                if (texture2D2 != texture2D)
                {
                    this.DrawPrimitives(texture2D, num3 + num4, k - num4);
                    texture2D = texture2D2;
                    num4 = k;
                }
            }

            this.DrawPrimitives(texture2D, num3 + num4, num2 - num4);
            if (numSprites <= 2048)
            {
                break;
            }

            numSprites -= 2048;
            num += 2048;
        }

        numSprites = 0;
    }

    public new void DrawPrimitives(Texture texture, int baseSprite, int batchSize)
    {
        if (customEffect != null)
        {
            customEffect.CurrentTechnique.Passes[_pass].Apply();
            base.GraphicsDevice.Textures[0] = texture;
            base.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, baseSprite * 4, 0, batchSize * 4, 0, batchSize * 2);

            return;
        }

        base.GraphicsDevice.Textures[0] = texture;
        base.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, baseSprite * 4, 0, batchSize * 4, 0, batchSize * 2);
    }
    #endregion

    // copied from Terraria.GameContent.TilePaintSystemV2.ARenderTargetHolder
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (Effect effect, int pass) PrepareShader(int paintColor, TreePaintingSettings settings)
    {
        Effect tileShader = Main.tileShader;
        tileShader.Parameters["leafHueTestOffset"]?.SetValue(settings.HueTestOffset);
        tileShader.Parameters["leafMinHue"]?.SetValue(settings.SpecialGroupMinimalHueValue);
        tileShader.Parameters["leafMaxHue"]?.SetValue(settings.SpecialGroupMaximumHueValue);
        tileShader.Parameters["leafMinSat"]?.SetValue(settings.SpecialGroupMinimumSaturationValue);
        tileShader.Parameters["leafMaxSat"]?.SetValue(settings.SpecialGroupMaximumSaturationValue);
        tileShader.Parameters["invertSpecialGroupResult"]?.SetValue(settings.InvertSpecialGroupResult);
        int index = Main.ConvertPaintIdToTileShaderIndex(paintColor, settings.UseSpecialGroups, settings.UseWallShaderHacks);
        tileShader.CurrentTechnique.Passes[index].Apply();

        return (tileShader, index);
    }
}
