using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ETD.Scripts.Common;
using ETD.Scripts.UI.View;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

namespace ETD.Scripts.Manager
{
   public class ResourcesManager : Singleton<ResourcesManager>
   {
      public static Dictionary<GoodType, Sprite> GoodSprites { get; private set; }
      public static Dictionary<ElementalType, Sprite[]> ElementalSprites { get; private set; }
      public static Dictionary<GradeType, Color> GradeColor { get; private set; }
      public static Dictionary<ColorType, Color> UIColor { get; private set; }
      public static Dictionary<AttributeType, Sprite> AttributeSprites { get; private set; }
      public static Dictionary<ParticleType, ParticleSystem> ParticleSystems { get; private set; }
      public static Dictionary<ElementalType, Color> ElementalColor { get; private set; }
      public static Material GrayScaleMaterial  { get; private set; }
      public static Color GrayScaleColor  { get; private set; }
      
      [Header("Prefabs")]
      public ViewSlotUI viewSlotUIPrefab;
      public ViewSlotElemental viewSlotElementalPrefab;
      public ViewDamageText viewDamageTextPrefab;

      [Space][Space]
      [Header("Element Images - Color")]
      public Sprite[] colorFireElements;
      public Sprite[] colorWaterElements;
      public Sprite[] colorWindElements;
      public Sprite[] colorLightElements;
      public Sprite[] colorLightingElements;
      public Sprite[] colorDarknessElements;

      [Space][Space]
      [Header("Element Colors")]
      public Color fireColor;
      public Color waterColor;
      public Color windColor;
      public Color lightingColor;
      public Color lightColor;
      public Color darknessColor;
      
      [Space][Space]
      [Header("Element Back Card Images")]
      public Sprite[] backCardImages;
      public Sprite[] backRuneCardImages;
   
      [Space][Space]
      [Header("Grade Colors")]
      public Color cColor;
      public Color bColor;
      public Color aColor;
      public Color sColor;
      public Color ssColor;
      public Color sssColor;
      
      [Space][Space]
      [Header("Rank Sprites")]
      public Sprite[] rankSprites;

      [Space][Space]
      [Header("Rune Images")] 
      public Sprite[] projectileRunes;
      public Sprite[] expansionRunes;
      public Sprite[] chainRunes;
      public Sprite[] amplifyRunes;
      public Sprite[] attackSpeedRunes;
      public Sprite[] durationRunes;
      public Sprite[] randomTag;

      [Space] [Space] 
      [Header("Upgrade Images")]
      public Sprite[] upgradeImages;
   
      [Space][Space]
      [Header("Banner Images")] 
      public Sprite[] dungeonBlurryBanners;

      [Space][Space]
      [Header("Good Images")] 
      public Sprite[] goodSprites;

      [Space][Space]
      [Header("Release Images")] 
      public Sprite[] releaseSprites;
      
      [Space][Space]
      [Header("Attribute Images")] 
      public Sprite[] attrSprites;

      [Space] [Space] [Header("Guild Images")]
      public Sprite[] guildMarkBackgroundSprites;
      public Sprite[] guildMarkMainSymbolSprites;
      public Sprite[] guildMarkSubSymbolSprites;
      public Sprite masterMark;
      public Sprite memberMark;
      
      [Space][Space]
      [Header("Bullet Particles")] 
      public ParticleSystem[] particleSystems;

      [Space][Space]
      [Header("Material")] 
      public Material grayScaleMaterial;

      [Space][Space]
      [Header("ETC Color")] 
      public Color grayScaleColor = Color.gray;
      public Color gradeDefaultColor = Color.gray;

      private void Awake()
      {
         ElementalSprites = new Dictionary<ElementalType, Sprite[]>
         {
            { ElementalType.Fire, colorFireElements },
            { ElementalType.Water, colorWaterElements },
            { ElementalType.Wind, colorWindElements },
            { ElementalType.Light, colorLightElements },
            { ElementalType.Lighting, colorLightingElements },
            { ElementalType.Darkness, colorDarknessElements }
         };
         GradeColor = new Dictionary<GradeType, Color>
         {
            { GradeType.C, cColor },
            { GradeType.B, bColor },
            { GradeType.A, aColor },
            { GradeType.S, sColor },
            { GradeType.SS, ssColor },
            { GradeType.SSS, sssColor },
         };
         
         ElementalColor = new Dictionary<ElementalType, Color>
         {
            { ElementalType.Fire, fireColor },
            { ElementalType.Water, waterColor },
            { ElementalType.Wind, windColor },
            { ElementalType.Light, lightColor },
            { ElementalType.Lighting, lightingColor },
            { ElementalType.Darkness, darknessColor }
         };

         UIColor = new Dictionary<ColorType, Color>
         {
            { ColorType.SkyBlue, new Color(188f / 255f, 246f / 255f, 247f / 255)},
            { ColorType.Red, new Color(195f / 255f, 58f / 255f, 59f / 255)},
         };

         GoodSprites = new Dictionary<GoodType, Sprite>();
         foreach (GoodType goodType in Enum.GetValues(typeof(GoodType)))
         {
            if(goodType == GoodType.None) continue;
            GoodSprites[goodType] = goodSprites[(int)goodType];
         }

         AttributeSprites = new Dictionary<AttributeType, Sprite>();
         foreach (AttributeType attrType in Enum.GetValues(typeof(AttributeType)))
         {
            if(attrType == AttributeType.None) continue;
            
            AttributeSprites[attrType] = attrSprites[(int)attrType];
         }

         ParticleSystems = new Dictionary<ParticleType, ParticleSystem>();
         foreach (ParticleType particleType in Enum.GetValues(typeof(ParticleType)))
         {
            ParticleSystems[particleType] = particleSystems[(int)particleType];
         }

         GrayScaleMaterial = grayScaleMaterial;
         GrayScaleColor = grayScaleColor;
      }

      public override void Init(CancellationTokenSource cts)
      {
      }

      public ParticleSystem GetParticle(string name)
      {
         return particleSystems.First(x => x.name == name);
      }

      public Color GetGradeColor(GradeType gradeType)
      {
         return gradeType switch
         {
            GradeType.C => cColor,
            GradeType.B => bColor,
            GradeType.A => aColor,
            GradeType.S => sColor,
            GradeType.SS => ssColor,
            GradeType.SSS => sssColor,
            _ => Color.white
         };
      }

      public Sprite GetUnlockImage(UnlockType type)
      {
         return type switch
         {
            UnlockType.Research => releaseSprites[0],
            UnlockType.Raid => releaseSprites[1],
            UnlockType.Pass => releaseSprites[2],
            UnlockType.GrowPass => releaseSprites[3],
            UnlockType.Guild => releaseSprites[4],
            _ => null
         };
      }

      [CanBeNull]
      public Sprite GetRankSprite(int index)
      {
         return index >= rankSprites.Length ? null : rankSprites[index];
      }
   }
}
