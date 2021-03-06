using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using FargowiltasSouls.Items.Accessories.Enchantments;
using FargowiltasSouls.Projectiles.Masomode;
using FargowiltasSouls.Projectiles.Champions;

namespace FargowiltasSouls.NPCs.Champions
{
    [AutoloadBossHead]
    public class LifeChampion : ModNPC
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Champion of Life");
            NPCID.Sets.TrailCacheLength[npc.type] = 6;
            NPCID.Sets.TrailingMode[npc.type] = 1;
        }

        public override void SetDefaults()
        {
            npc.width = 110;
            npc.height = 110;
            npc.damage = 160;
            npc.defense = 0;
            npc.lifeMax = 35000;
            npc.HitSound = SoundID.NPCHit5;
            npc.DeathSound = SoundID.NPCDeath7;
            npc.noGravity = true;
            npc.noTileCollide = true;
            npc.knockBackResist = 0f;
            npc.lavaImmune = true;
            npc.aiStyle = -1;
            npc.value = Item.buyPrice(0, 15);
            npc.boss = true;

            npc.buffImmune[BuffID.Chilled] = true;
            npc.buffImmune[BuffID.OnFire] = true;
            npc.buffImmune[BuffID.Suffocation] = true;
            npc.buffImmune[mod.BuffType("Lethargic")] = true;
            npc.buffImmune[mod.BuffType("ClippedWings")] = true;
            npc.GetGlobalNPC<FargoSoulsGlobalNPC>().SpecialEnchantImmune = true;

            music = mod.GetSoundSlot(SoundType.Music, "Sounds/Music/Champions");
            musicPriority = MusicPriority.BossHigh;
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            cooldownSlot = 1;
            return true;
        }

        public override void AI()
        {
            if (npc.localAI[3] == 0) //just spawned
            {
                npc.TargetClosest(false);
                Movement(Main.player[npc.target].Center, 0.8f, 32f);
                if (npc.Distance(Main.player[npc.target].Center) < 2000)
                    npc.localAI[3] = 1;
                else
                    return;

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    if (FargoSoulsWorld.MasochistMode)
                        Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<LifeRitual>(), npc.damage / 2, 0f, Main.myPlayer, 0f, npc.whoAmI);
                }
            }

            EModeGlobalNPC.championBoss = npc.whoAmI;

            Player player = Main.player[npc.target];
            Vector2 targetPos;

            if (npc.HasValidTarget && npc.Distance(player.Center) < 2500)
                npc.timeLeft = 600;

            switch ((int)npc.ai[0])
            {
                case -1: //heal
                    npc.localAI[2] = 1;

                    npc.velocity *= 0.97f;

                    if (++npc.ai[1] == 180) //heal up
                    {
                        Main.PlaySound(SoundID.Roar, npc.Center, 2); //arte scream

                        int heal = npc.lifeMax - npc.life;
                        npc.life += heal;
                        CombatText.NewText(npc.Hitbox, CombatText.HealLife, heal);

                        const int num226 = 80;
                        for (int num227 = 0; num227 < num226; num227++)
                        {
                            Vector2 vector6 = Vector2.UnitX * 40f;
                            vector6 = vector6.RotatedBy(((num227 - (num226 / 2 - 1)) * 6.28318548f / num226), default(Vector2)) + npc.Center;
                            Vector2 vector7 = vector6 - npc.Center;
                            int num228 = Dust.NewDust(vector6 + vector7, 0, 0, 174, 0f, 0f, 0, default(Color), 3f);
                            Main.dust[num228].noGravity = true;
                            Main.dust[num228].velocity = vector7;
                        }
                    }
                    else if (npc.ai[1] > 240)
                    {
                        npc.ai[0] = npc.ai[3];
                        npc.ai[1] = 0;
                        npc.ai[2] = 0;
                        npc.ai[3] = 0;
                        npc.netUpdate = true;
                    }
                    break;

                case 0: //float over player
                    if (!player.active || player.dead || Vector2.Distance(npc.Center, player.Center) > 2500f) //despawn code
                    {
                        npc.TargetClosest(false);
                        if (npc.timeLeft > 30)
                            npc.timeLeft = 30;

                        npc.noTileCollide = true;
                        npc.noGravity = true;
                        npc.velocity.Y -= 1f;

                        break;
                    }
                    
                    targetPos = player.Center;
                    targetPos.Y -= 300;
                    if (npc.Distance(targetPos) > 50)
                        Movement(targetPos, 0.16f, 24f, true);

                    if (++npc.ai[1] > 180)
                    {
                        npc.TargetClosest();
                        npc.ai[0]++;
                        npc.ai[1] = 0;
                        npc.ai[2] = 0;
                        npc.ai[3] = 0;
                        npc.netUpdate = true;
                    }

                    if (npc.localAI[2] == 0 && npc.life < npc.lifeMax / 2)
                    {
                        float buffer = npc.ai[0];
                        npc.ai[0] = -1;
                        npc.ai[1] = 0;
                        npc.ai[2] = 0;
                        npc.ai[3] = buffer;
                    }
                    break;

                case 1: //boundary
                    npc.velocity *= 0.95f;
                    if (++npc.ai[1] > (npc.localAI[2] == 1 ? 2 : 3))
                    {
                        Main.PlaySound(SoundID.Item12, npc.Center);
                        npc.ai[1] = 0;
                        npc.ai[2] -= (float)Math.PI / 4 / 457 * npc.ai[3];
                        if (npc.ai[2] < -(float)Math.PI)
                            npc.ai[2] += (float)Math.PI * 2;
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int max = npc.localAI[2] == 1 ? 5 : 4;
                            for (int i = 0; i < max; i++)
                            {
                                Projectile.NewProjectile(npc.Center, new Vector2(6f, 0).RotatedBy(npc.ai[2] + Math.PI / max * 2 * i),
                                    ModContent.ProjectileType<ChampionBee>(), npc.damage / 4, 0f, Main.myPlayer);
                            }
                        }
                    }
                    if (++npc.ai[3] > 300)
                    {
                        npc.TargetClosest();
                        npc.ai[0]++;
                        npc.ai[1] = 0;
                        npc.ai[2] = 0;
                        npc.ai[3] = 0;
                        npc.netUpdate = true;
                    }
                    break;

                case 2:
                    if (npc.ai[3] == 0)
                    {
                        if (!player.active || player.dead || Vector2.Distance(npc.Center, player.Center) > 2500f) //despawn code
                        {
                            npc.TargetClosest(false);
                            if (npc.timeLeft > 30)
                                npc.timeLeft = 30;

                            npc.noTileCollide = true;
                            npc.noGravity = true;
                            npc.velocity.Y -= 1f;

                            return;
                        }

                        if (npc.ai[2] == 0)
                            npc.ai[2] = npc.Center.Y; //store arena height
                        
                        if (npc.Center.Y > npc.ai[2] + 1000) //now below arena, track player
                        {
                            targetPos = new Vector2(player.Center.X, npc.ai[2] + 1100);
                            Movement(targetPos, 1.2f, 24f);

                            if (Math.Abs(player.Center.X - npc.Center.X) < npc.width / 2
                                && ++npc.ai[1] > (npc.localAI[2] == 1 ? 30 : 60)) //in position under player
                            {
                                Main.PlaySound(SoundID.Item92, npc.Center);

                                npc.ai[3]++;
                                npc.ai[1] = 0;
                                npc.netUpdate = true;
                            }
                        }
                        else //drop below arena
                        {
                            npc.velocity.X *= 0.95f;
                            npc.velocity.Y += 0.6f;
                        }
                    }
                    else
                    {
                        npc.velocity.X = 0;
                        npc.velocity.Y = -36f;

                        if (++npc.ai[1] > 1) //spawn pixies
                        {
                            npc.ai[1] = 0;
                            npc.localAI[0] = npc.localAI[0] == 1 ? -1 : 1; //alternate sides
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                int n = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, ModContent.NPCType<LesserFairy>(), npc.whoAmI, Target: npc.target);
                                if (n != Main.maxNPCs)
                                {
                                    Main.npc[n].velocity = 5f * Vector2.UnitX.RotatedBy(Math.PI * (Main.rand.NextDouble() - 0.5));
                                    Main.npc[n].velocity.X *= npc.localAI[0];

                                    if (Main.netMode == NetmodeID.Server)
                                    {
                                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, n);
                                    }
                                }
                            }
                        }

                        if (npc.Center.Y < player.Center.Y - 600) //dash ended
                        {
                            npc.velocity.Y = 0f;
                            npc.localAI[0] = 0f;

                            npc.TargetClosest();
                            npc.ai[0]++;
                            npc.ai[1] = 0;
                            npc.ai[2] = 0;
                            npc.ai[3] = 0;
                            npc.netUpdate = true;
                        }
                    }
                    break;

                case 3:
                    goto case 0;

                case 4: //beetle swarm
                    npc.velocity *= 0.9f;

                    if (npc.ai[3] == 0)
                        npc.ai[3] = npc.Center.X < player.Center.X ? -1 : 1;

                    if (++npc.ai[2] > (npc.localAI[2] == 1 ? 35 : 45))
                    {
                        npc.ai[2] = 0;
                        Main.PlaySound(SoundID.Item92, npc.Center);

                        if (npc.localAI[0] > 0)
                            npc.localAI[0] = -1;
                        else
                            npc.localAI[0] = 1;

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Vector2 projTarget = npc.Center;
                            projTarget.X += 1200 * npc.ai[3];
                            projTarget.Y += 1200 * -npc.localAI[0];
                            for (int i = 0; i < 20; i++)
                            {
                                projTarget.Y += 250 * npc.localAI[0];
                                Vector2 speed = (projTarget - npc.Center) / 40;
                                float ai0 = (npc.localAI[2] == 1 ? 9 : 6) * -npc.ai[3]; //x speed of beetles
                                float ai1 = 6 * -npc.localAI[0]; //y speed of beetles
                                Projectile.NewProjectile(npc.Center, speed, ModContent.ProjectileType<ChampionBeetle>(), npc.damage / 4, 0f, Main.myPlayer, ai0, ai1);
                            }
                        }
                    }

                    if (++npc.ai[1] > 360)
                    {
                        npc.localAI[0] = 0;

                        npc.TargetClosest();
                        npc.ai[0]++;
                        npc.ai[1] = 0;
                        npc.ai[2] = 0;
                        npc.ai[3] = 0;
                        npc.netUpdate = true;
                    }
                    break;

                case 5:
                    goto case 0;

                case 6:
                    npc.velocity *= 0.98f;

                    if (++npc.ai[2] > 60)
                    {
                        if (++npc.ai[3] > (npc.localAI[2] == 1 ? 4 : 7)) //spray fireballs that home down
                        {
                            npc.ai[3] = 0;
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                //spawn anywhere above self
                                Vector2 target = new Vector2(Main.rand.NextFloat(1000), 0).RotatedBy(Main.rand.NextDouble() * -Math.PI);
                                Vector2 speed = 2 * target / 60;
                                float acceleration = -speed.Length() / 60;
                                Projectile.NewProjectile(npc.Center, speed, ModContent.ProjectileType<LifeFireball>(),
                                    npc.damage / 4, 0f, Main.myPlayer, 60f, acceleration);
                            }
                        }

                        if (npc.ai[2] > (npc.localAI[2] == 1 ? 120 : 100))
                        {
                            npc.netUpdate = true;
                            npc.ai[2] = 0;
                        }
                    }

                    if (++npc.ai[1] > 480)
                    {
                        npc.TargetClosest();
                        npc.ai[0]++;
                        npc.ai[1] = 0;
                        npc.ai[2] = 0;
                        npc.ai[3] = 0;
                        npc.netUpdate = true;
                    }
                    break;

                case 7:
                    goto case 0;

                case 8: //deathray spin
                    npc.velocity *= 0.95f;

                    npc.ai[3] +=  (float)Math.PI * 2 / (npc.localAI[2] == 1 ? -300 : 360);

                    if (--npc.ai[2] < 0)
                    {
                        npc.ai[2] = 60;
                        npc.localAI[1] = npc.localAI[1] == 0 ? 1 : 0;

                        if (npc.ai[1] < 360 && Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int type = npc.localAI[1] == 1 ? ModContent.ProjectileType<LifeDeathraySmall>() : ModContent.ProjectileType<LifeDeathray>();
                            int max = npc.localAI[2] == 1 ? 8 : 4;
                            for (int i = 0; i < max; i++)
                            {
                                float offset = (float)Math.PI * 2 / max * i;
                                Projectile.NewProjectile(npc.Center, Vector2.UnitX.RotatedBy(npc.ai[3] + offset),
                                    type, npc.damage / 4, 0f, Main.myPlayer, offset, npc.whoAmI);
                            }
                        }
                    }

                    if (++npc.ai[1] > 390)
                    {
                        npc.TargetClosest();
                        npc.ai[0]++;
                        npc.ai[1] = 0;
                        npc.ai[2] = 0;
                        npc.ai[3] = 0;
                        npc.localAI[1] = 0;
                        npc.netUpdate = true;
                    }
                    break;

                case 9:
                    goto case 2;

                case 10:
                    goto case 0;

                case 11: //cactus mines
                    npc.velocity *= 0.98f;

                    if (++npc.ai[2] > (npc.localAI[2] == 1 ? 75 : 100))
                    {
                        if (++npc.ai[3] > 5) //spray mines that home down
                        {
                            npc.ai[3] = 0;

                            Main.PlaySound(SoundID.Item12, npc.Center);

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Vector2 target = player.Center - npc.Center;
                                target.X += Main.rand.Next(-75, 76);
                                target.Y += Main.rand.Next(-75, 76);

                                Vector2 speed = 2 * target / 90;
                                float acceleration = -speed.Length() / 90;

                                Projectile.NewProjectile(npc.Center, speed, ModContent.ProjectileType<CactusMine>(),
                                    npc.damage / 4, 0f, Main.myPlayer, 0f, acceleration);
                            }
                        }

                        if (npc.ai[2] > 130)
                        {
                            npc.netUpdate = true;
                            npc.ai[2] = 0;
                        }
                    }

                    if (++npc.ai[1] > 480)
                    {
                        npc.TargetClosest();
                        npc.ai[0]++;
                        npc.ai[1] = 0;
                        npc.ai[2] = 0;
                        npc.ai[3] = 0;
                        npc.netUpdate = true;
                    }
                    break;

                default:
                    npc.ai[0] = 0;
                    goto case 0;
            }

            for (int i = 0; i < 3; i++)
            {
                int d = Dust.NewDust(npc.position, npc.width, npc.height, 87, 0f, 0f, 0, default(Color), 1.5f);
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity *= 4f;
            }
        }

        private void Movement(Vector2 targetPos, float speedModifier, float cap = 12f, bool fastY = false)
        {
            if (npc.Center.X < targetPos.X)
            {
                npc.velocity.X += speedModifier;
                if (npc.velocity.X < 0)
                    npc.velocity.X += speedModifier * 2;
            }
            else
            {
                npc.velocity.X -= speedModifier;
                if (npc.velocity.X > 0)
                    npc.velocity.X -= speedModifier * 2;
            }
            if (npc.Center.Y < targetPos.Y)
            {
                npc.velocity.Y += fastY ? speedModifier * 2 : speedModifier;
                if (npc.velocity.Y < 0)
                    npc.velocity.Y += speedModifier * 2;
            }
            else
            {
                npc.velocity.Y -= fastY ? speedModifier * 2 : speedModifier;
                if (npc.velocity.Y > 0)
                    npc.velocity.Y -= speedModifier * 2;
            }
            if (Math.Abs(npc.velocity.X) > cap)
                npc.velocity.X = cap * Math.Sign(npc.velocity.X);
            if (Math.Abs(npc.velocity.Y) > cap)
                npc.velocity.Y = cap * Math.Sign(npc.velocity.Y);
        }

        public override bool StrikeNPC(ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit)
        {
            damage /= 10;
            return true;
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            if (FargoSoulsWorld.MasochistMode)
                target.AddBuff(mod.BuffType("Purified"), 300);
        }

        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.SuperHealingPotion;
        }

        public override void NPCLoot()
        {
            FargoSoulsWorld.downedChampions[4] = true;
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.WorldData); //sync world

            //Item.NewItem(npc.position, npc.Size, ModContent.ItemType<LifeForce>());
            int[] drops = {
                ModContent.ItemType<PumpkinEnchant>(),
                ModContent.ItemType<BeeEnchant>(),
                ModContent.ItemType<SpiderEnchant>(),
                ModContent.ItemType<TurtleEnchant>(),
                ModContent.ItemType<BeetleEnchant>(),
            };
            int lastDrop = -1; //don't drop same ench twice
            for (int i = 0; i < 2; i++)
            {
                int thisDrop = Main.rand.Next(drops.Length);

                if (lastDrop == thisDrop) //try again
                {
                    if (++thisDrop >= drops.Length) //drop first ench in line if looped past array
                        thisDrop = 0;
                }

                lastDrop = thisDrop;
                Item.NewItem(npc.position, npc.Size, drops[thisDrop]);
            }
        }

        public override Color? GetAlpha(Color drawColor)
        {
            return Color.White * npc.Opacity;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture2D13 = Main.npcTexture[npc.type];
            //int num156 = Main.npcTexture[npc.type].Height / Main.npcFrameCount[npc.type]; //ypos of lower right corner of sprite to draw
            //int y3 = num156 * npc.frame.Y; //ypos of upper left corner of sprite to draw
            Rectangle rectangle = npc.frame;//new Rectangle(0, y3, texture2D13.Width, num156);
            Vector2 origin2 = rectangle.Size() / 2f;

            Color color26 = lightColor;
            color26 = npc.GetAlpha(color26);

            SpriteEffects effects = npc.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            for (int i = 0; i < NPCID.Sets.TrailCacheLength[npc.type]; i++)
            {
                Color color27 = color26 * 0.5f;
                color27 *= (float)(NPCID.Sets.TrailCacheLength[npc.type] - i) / NPCID.Sets.TrailCacheLength[npc.type];
                Vector2 value4 = npc.oldPos[i];
                float num165 = npc.rotation; //npc.oldRot[i];
                Main.spriteBatch.Draw(texture2D13, value4 + npc.Size / 2f - Main.screenPosition + new Vector2(0, npc.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), color27, num165, origin2, npc.scale, effects, 0f);
            }

            Main.spriteBatch.Draw(texture2D13, npc.Center - Main.screenPosition + new Vector2(0f, npc.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), npc.GetAlpha(lightColor), npc.rotation, origin2, npc.scale, effects, 0f);
            return false;
        }
    }
}