﻿using UnityEngine;

namespace KsmUI
{
    public class Kolor
    {
        public readonly string name;
        public readonly string hex;
        public readonly Color color;

        public Kolor(string name, string hex, Color color)
        {
            this.name = name;
            this.hex = hex;
            this.color = color;
        }

        public override string ToString()
        {
            return name;
        }

        /// <summary>white, use this in the Color() methods if no color tag is to be applied</summary>
        public static readonly Kolor White = new Kolor("White", "#FFFFFF", new Color(1.000f, 1.000f, 1.000f));
        /// <summary>green whith slightly less red than the ksp ui default (CCFF00), for better contrast with yellow</summary>
        public static readonly Kolor Green = new Kolor("Green", "#88FF00", new Color(0.533f, 1.000f, 0.000f));
        /// <summary>ksp ui yellow</summary>
        public static readonly Kolor Yellow = new Kolor("Yellow", "#FFD200", new Color(1.000f, 0.824f, 0.000f));
        /// <summary>ksp ui orange</summary>
        public static readonly Kolor Orange = new Kolor("Orange", "#FF8000", new Color(1.000f, 0.502f, 0.000f));
        /// <summary>custom red</summary>
        public static readonly Kolor Red = new Kolor("Red", "#FF3333", new Color(1.000f, 0.200f, 0.200f));
        /// <summary>ksp science color</summary>
        public static readonly Kolor Science = new Kolor("Science", "#6DCFF6", new Color(0.427f, 0.812f, 0.965f));
        /// <summary>cyan</summary>
        public static readonly Kolor Cyan = new Kolor("Cyan", "#00FFFF", new Color(0.000f, 1.000f, 1.000f));
        /// <summary>light grey</summary>
        public static readonly Kolor LightGrey = new Kolor("LightGrey", "#CCCCCC", new Color(0.800f, 0.800f, 0.800f));
        /// <summary>dark grey</summary>
        public static readonly Kolor DarkGrey = new Kolor("DarkGrey", "#999999", new Color(0.600f, 0.600f, 0.600f));
        /// <summary>very dark grey</summary>
        public static readonly Kolor NearBlack = new Kolor("NearBlack", "#434343", new Color(0.263f, 0.263f, 0.263f));
        /// <summary>0.2 alpha black</summary>
        public static readonly Kolor Box = new Kolor("Box", "#FFFFFF", new Color(0.000f, 0.000f, 0.000f, 0.200f));
        /// <summary>0.5 alpha black</summary>
        public static readonly Kolor BoxSelected = new Kolor("BoxSelected", "#FFFFFF", new Color(0.000f, 0.000f, 0.000f, 0.500f));
        /// <summary></summary>
        public static Kolor PosRate => Green;
        public static Kolor NegRate => Orange;

        public static Kolor Parse(string kolorName)
        {
            switch (kolorName)
            {
                case "White": return White;
                case "Green": return Green;
                case "Yellow": return Yellow;
                case "Orange": return Orange;
                case "Red": return Red;
                case "Science": return Science;
                case "Cyan": return Cyan;
                case "LightGrey": return LightGrey;
                case "DarkGrey": return DarkGrey;
                case "NearBlack": return NearBlack;
                case "PosRate": return PosRate;
                case "NegRate": return NegRate;
                case "Box": return Box;
                case "BoxSelected": return BoxSelected;
                default: return null;
            }
        }

        public static implicit operator Color(Kolor kolor) => kolor.color;
    }
}
