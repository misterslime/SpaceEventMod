using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.UI;

namespace SpaceEventMod.Common;

// theres no way this is at all necessary istg
// basically just lets me use a Vector2 as a map icon's scale
internal struct ModifiedMapDrawContext
{
    public struct DrawResult
    {
        public static readonly DrawResult Culled = new DrawResult(isMouseOver: false);
        public readonly bool IsMouseOver;

        public DrawResult(bool isMouseOver)
        {
            IsMouseOver = isMouseOver;
        }
    }

    private readonly Vector2 _mapPosition;
    private readonly Vector2 _mapOffset;
    private readonly Rectangle? _clippingRect;
    private readonly float _mapScale;
    private readonly Vector2 _drawScale;

    public ModifiedMapDrawContext(MapOverlayDrawContext context)
    {
        _mapPosition = context.MapPosition;
        _mapOffset = context.MapOffset;
        _clippingRect = context.ClippingRectangle;
        _mapScale = context.MapScale;
        _drawScale = new Vector2(context.DrawScale);
    }

    public DrawResult Draw(Texture2D texture, Vector2 position, Color color, SpriteFrame frame, Vector2 scaleIfNotSelected, Vector2 scaleIfSelected, Alignment alignment, SpriteEffects spriteEffects = SpriteEffects.None)
    {
        position = (position - _mapPosition) * _mapScale + _mapOffset;
        if (_clippingRect.HasValue && !_clippingRect.Value.Contains(position.ToPoint()))
            return DrawResult.Culled;

        Rectangle sourceRectangle = frame.GetSourceRectangle(texture);
        Vector2 vector = sourceRectangle.Size() * alignment.OffsetMultiplier;
        Vector2 position2 = position;

        Vector2 scale = _drawScale * scaleIfNotSelected;
        Vector2 vector2 = position - vector * scale;

        bool mouseSelected = new Rectangle((int)vector2.X, (int)vector2.Y, (int)((float)sourceRectangle.Width * scale.X), (int)((float)sourceRectangle.Height * scale.Y)).Contains(Main.MouseScreen.ToPoint());

        if (mouseSelected)
            scale = _drawScale * scaleIfSelected;

        Main.spriteBatch.Draw(texture, position2, sourceRectangle, color, 0f, vector, scale, spriteEffects, 0f);
        return new DrawResult(mouseSelected);
    }
}
