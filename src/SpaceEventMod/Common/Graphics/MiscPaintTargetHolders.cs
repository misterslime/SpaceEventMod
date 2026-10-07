using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;

namespace SpaceEventMod.Common.Graphics;

// borrowed and adapted from https://github.com/flye-name/solstice/blob/master/src/Solstice/Content/Aerie/Placements/AerieLeafLitter.cs#L441
// with permission
internal class MiscPaintSystem(Asset<Texture2D> asset)
{
    private readonly Dictionary<int, MiscRenderTargetHolder> _paintCache = [];
    private Asset<Texture2D> _asset = asset;

    private class MiscRenderTargetHolder(int paintColor, Asset<Texture2D> asset, int copySettingsFrom = -1) : TilePaintSystemV2.ARenderTargetHolder
    {
        public int PaintColor { get; private set; } = paintColor;

        public TreePaintingSettings PaintSettings { get; private set; } = TreePaintSystemData.GetTileSettings(copySettingsFrom, 0);

        public Asset<Texture2D> Texture { get; set; } = asset;

        public override void Prepare()
        {
            Texture.Wait?.Invoke();
            PrepareTextureIfNecessary(Texture.Value);
        }

        public override void PrepareShader()
        {
            PrepareShader(PaintColor, PaintSettings);
        }
    }

    public bool TryGetPaintTexture(int paintColor, [NotNullWhen(true)] out Texture2D? texture)
    {
        texture = null;

        if (_paintCache.TryGetValue(paintColor, out MiscRenderTargetHolder? holder) &&
            holder.IsReady)
        {
            texture = holder.Target;

            return true;
        }

        var newHolder = new MiscRenderTargetHolder(paintColor, _asset);

        _paintCache[paintColor] = newHolder;

        Main.instance.TilePaintSystem._requests.Add(newHolder);

        return false;
    }
}
