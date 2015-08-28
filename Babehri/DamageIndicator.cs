﻿using System;
using System.Globalization;
using System.Linq;
using System.Security.Permissions;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace Babehri
{
    [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
    internal static class DamageIndicator
    {
        public delegate float DamageToUnitDelegate(Obj_AI_Hero hero);

        private const int XOffset = 10;
        private const int YOffset = 40;
        private const int Width = 103;
        private const int Height = 8;

        private static readonly Render.Text Text = new Render.Text(
            0, 0, "", 14, new ColorBGRA(255, 0, 0, 255), "Tahoma");

        private static readonly Render.Rectangle DamageBar = new Render.Rectangle(0, 0, 1, 8, Color.White);
        private static readonly Render.Line HealthLine = new Render.Line(Vector2.Zero, Vector2.Zero, 1, Color.White);

        static DamageIndicator()
        {
            Drawing.OnDraw += Drawing_OnDraw;
        }

        public static bool PredictedHealth
        {
            get { return Program.Menu.Item("HPColor").GetValue<Circle>().Active; }
        }

        public static bool Fill
        {
            get { return Program.Menu.Item("FillColor").GetValue<Circle>().Active; }
        }

        public static System.Drawing.Color HealthColor
        {
            get { return Program.Menu.Item("HPColor").GetValue<Circle>().Color; }
        }

        public static System.Drawing.Color DamageColor
        {
            get { return Program.Menu.Item("FillColor").GetValue<Circle>().Color; }
        }

        public static bool Enabled
        {
            get { return Program.Menu.Item("DmgEnabled").GetValue<bool>(); }
        }

        public static DamageToUnitDelegate DamageToUnit { get; set; }

        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (
                var unit in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValid && h.IsHPBarRendered && h.IsEnemy))
            {
                var barPos = unit.HPBarPosition;
                var damage = DamageToUnit(unit);
                var percentHealthAfterDamage = Math.Max(0, unit.Health - damage) / unit.MaxHealth;
                var yPos = barPos.Y + YOffset;
                var xPosDamage = barPos.X + XOffset + Width * percentHealthAfterDamage;
                var xPosCurrentHp = barPos.X + XOffset + Width * unit.Health / unit.MaxHealth;

                if (damage > unit.Health)
                {
                    Text.X = (int) barPos.X + XOffset;
                    Text.Y = (int) barPos.Y + YOffset;
                    Text.text = ((int) (unit.Health - damage)).ToString(CultureInfo.InvariantCulture);
                    Text.OnEndScene();
                }

                if (PredictedHealth)
                {
                    HealthLine.Start = new Vector2(xPosDamage, yPos);
                    HealthLine.End = new Vector2(xPosDamage, yPos + Height);
                    HealthLine.Width = 2;
                    HealthLine.Color = HealthColor.ToBGRA();
                    HealthLine.OnEndScene();
                }


                if (Fill)
                {
                    var differenceInHp = xPosCurrentHp - xPosDamage;
                    DamageBar.Color = DamageColor.ToBGRA();
                    DamageBar.X = (int) (barPos.X + 9 + (107 * percentHealthAfterDamage));
                    DamageBar.Y = (int) yPos;
                    DamageBar.Width = (int) Math.Round(differenceInHp);
                    DamageBar.Height = Height;
                    DamageBar.OnEndScene();
                }
            }
        }
    }
}