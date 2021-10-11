using System;
using R2API;
using RoR2;
using UnityEngine;
using BepInEx;
using R2API.Utils;
using EntityStates;
using EntityStates.Toolbot;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2.Projectile;

namespace cope
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.FMRadio11.cope", "Savage MUL-T", "0.6.9")]
    [R2APISubmoduleDependency(new string[]
    {
        "BuffAPI",
    })]
    public class Why : BaseUnityPlugin
    {
        public void Awake()
        {
            SkillIssue();
            PewPew();
            Buff();
            On.RoR2.CharacterBody.RecalculateStats += new On.RoR2.CharacterBody.hook_RecalculateStats(CharacterBody_RecalculateStats);
            On.RoR2.BodyCatalog.Init += orig =>
            {
                orig();
                BodyCatalog.FindBodyPrefab("ToolbotBody").GetComponent<SkillLocator>().primary.skillFamily.variants[2].skillDef.baseRechargeInterval = 1;
            };
        }
        public static void Buff()
        {
            copeBuff = ScriptableObject.CreateInstance<BuffDef>();
            copeBuff.buffColor = new Color(0.3f, 1f, 0.3f);
            copeBuff.canStack = false;
            copeBuff.eliteDef = null;
            copeBuff.iconSprite = Resources.Load<Sprite>("Textures/bufficons/texbufflunarshellicon");
            copeBuff.isDebuff = true;
            copeBuff.name = "SkillIssue";
            BuffAPI.Add(new CustomBuff(copeBuff));
            copeBuff2 = ScriptableObject.CreateInstance<BuffDef>();
            copeBuff2.buffColor = new Color(0.3f, 1f, 0.3f);
            copeBuff2.canStack = false;
            copeBuff2.eliteDef = null;
            copeBuff2.iconSprite = Resources.Load<Sprite>("Textures/bufficons/texbufflunarshellicon");
            copeBuff2.isDebuff = true;
            copeBuff2.name = "SkillIssue";
            BuffAPI.Add(new CustomBuff(copeBuff2));
            pewpewBuff = ScriptableObject.CreateInstance<BuffDef>();
            pewpewBuff.buffColor = new Color(1, 0.3f, 0.3f);
            pewpewBuff.canStack = false;
            pewpewBuff.eliteDef = null;
            pewpewBuff.iconSprite = Resources.Load<Sprite>("Textures/bufficons/texmovespeedbufficon");
            pewpewBuff.isDebuff = true;
            pewpewBuff.name = "Woolie";
            BuffAPI.Add(new CustomBuff(pewpewBuff));
        }
        private static void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            if (self && self.HasBuff(copeBuff))
            {
                Reflection.SetPropertyValue(self, "armor", self.armor + 100);
            }
            if (self && self.HasBuff(copeBuff2))
            {
                Reflection.SetPropertyValue(self, "armor", self.armor + 200);
            }
            if (self && self.HasBuff(pewpewBuff))
            {
                Reflection.SetPropertyValue(self, "attackSpeed", self.attackSpeed + 1);
            }
        }
        public static void SkillIssue()
        {
            On.EntityStates.Toolbot.BaseNailgunState.OnEnter += (orig, self) =>
            {
                BaseNailgunState.procCoefficient = 1;
                BaseNailgunState.spreadBloomValue = 0.02f;
                BaseNailgunState.spreadYawScale = 0.05f;
                BaseNailgunState.maxDistance = 420;
                orig(self);
            };
            On.EntityStates.Toolbot.FireSpear.OnEnter += (orig, self) =>
            {
                self.damageCoefficient = 4;
                orig(self);
            };
            On.EntityStates.Toolbot.FireBuzzsaw.OnEnter += (orig, self) =>
            {
                FireBuzzsaw.procCoefficientPerSecond = 20;
                orig(self);
            };
            On.EntityStates.Toolbot.FireBuzzsaw.FixedUpdate += (orig, self) =>
            {
                self.outer.commonComponents.characterBody.healthComponent.AddBarrierAuthority(0.003f * self.outer.commonComponents.characterBody.maxBarrier);
                orig(self);
            };
            On.EntityStates.Toolbot.AimGrenade.OnEnter += (orig, self) =>
            {
                self.damageCoefficient = 3.99f;
                orig(self);
            };
            IL.EntityStates.Toolbot.ToolbotDash.OnEnter += (il) =>
            {
                ILCursor roll = new ILCursor(il);
                roll.Emit(OpCodes.Ldarg_0);
                roll.EmitDelegate<Action<ToolbotDash>>(delegate (ToolbotDash del)
                {
                    if (del.outer)
                    {
                        if (del.outer.commonComponents.characterBody)
                        {
                            del.outer.commonComponents.characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
                        }
                    }
                });
            };
            IL.EntityStates.Toolbot.ToolbotDash.OnExit += (il) =>
            {
                ILCursor roll = new ILCursor(il);
                roll.Emit(OpCodes.Ldarg_0);
                roll.EmitDelegate<Action<ToolbotDash>>(delegate (ToolbotDash del)
                {
                    if (del.outer)
                    {
                        if (del.outer.commonComponents.characterBody)
                        {
                            del.outer.commonComponents.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
                            del.outer.commonComponents.characterBody.AddTimedBuff(copeBuff2, 3);
                        }
                    }
                });
            };
            On.EntityStates.Toolbot.ToolbotDash.OnEnter += (orig, self) =>
            {
                self.speedMultiplier *= 2;
                orig(self);
            };
            On.EntityStates.Toolbot.ToolbotDash.OnExit += (orig, self) =>
            {
                self.speedMultiplier /= 2;
                orig(self);
            };
            IL.EntityStates.Toolbot.StartToolbotStanceSwap.OnEnter += (il) =>
            {
                ILCursor roll = new ILCursor(il);
                roll.Emit(OpCodes.Ldarg_0);
                roll.EmitDelegate<Action<ToolbotDash>>(delegate (ToolbotDash del)
                {
                    if (del.outer)
                    {
                        if (del.outer.commonComponents.characterBody)
                        {
                            del.outer.commonComponents.characterBody.AddTimedBuff(pewpewBuff, 2);
                        }
                    }
                });
            };
            IL.EntityStates.Toolbot.ToolbotDualWieldStart.OnEnter += (il) =>
            {
                ILCursor roll = new ILCursor(il);
                roll.Emit(OpCodes.Ldarg_0);
                roll.EmitDelegate<Action<ToolbotDash>>(delegate (ToolbotDash del)
                {
                    if (del.outer)
                    {
                        if (del.outer.commonComponents.characterBody)
                        {
                            del.outer.commonComponents.characterBody.AddBuff(copeBuff);
                        }
                    }
                });
            };
            IL.EntityStates.Toolbot.ToolbotDualWield.OnExit += (il) =>
            {
                ILCursor roll = new ILCursor(il);
                roll.Emit(OpCodes.Ldarg_0);
                roll.EmitDelegate<Action<ToolbotDash>>(delegate (ToolbotDash del)
                {
                    if (del.outer)
                    {
                        if (del.outer.commonComponents.characterBody)
                        {
                            del.outer.commonComponents.characterBody.RemoveBuff(copeBuff);
                        }
                    }
                });
            };
            Resources.Load<GameObject>("prefabs/projectiles/cryocanisterprojectile").GetComponent<ProjectileImpactExplosion>().childrenDamageCoefficient = 0.37593984962406015037593984962406f;
            Resources.Load<GameObject>("prefabs/projectiles/cryocanisterbombletsprojectile").GetComponent<ProjectileImpactExplosion>().blastProcCoefficient = 1;
        }
        public static void PewPew()
        {

        }
        public static BuffDef copeBuff;
        public static BuffDef copeBuff2;
        public static BuffDef pewpewBuff;
    }
}
