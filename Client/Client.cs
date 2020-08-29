using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

using Nexd.ESX.Client;

namespace Eternar.RCCar.Client
{
    public class Client : BaseScript
    {
        public int Entity;
        public int Driver;

        public int TabletEntity;
        public int Camera;

        public List<uint> CachedModels = new List<uint>();
        public List<string> CachedAnims = new List<string>();

        public bool CameraState;

        public const float MaxDistance = 250.0f;

        public Client()
        {
            EventHandlers["Eternar::RCCar::PLACE"] += new Action(Start);
            EventHandlers["Eternar::RCCar::CHARGE"] += new Action(Charge);
            Tick += OnGameFrame;
        }

        private async Task OnGameFrame()
        {
            if (!DoesEntityExist(Entity) && !DoesEntityExist(Driver))
                return;

            Vector3 PlayerPos = GetEntityCoords(PlayerPedId(), true);
            Vector3 CarPos = GetEntityCoords(Entity, true);
            float distanceCheck = GetDistanceBetweenCoords(PlayerPos.X, PlayerPos.Y, PlayerPos.Z, CarPos.X, CarPos.Y, CarPos.Z, true);

            if(GetVehicleEngineHealth(Entity) <= 649.0f)
            {
                ExplodeVehicle(Entity, true, false);
            }

            DrawInstructions(distanceCheck);
            HandleKeys(distanceCheck);

            if (CameraState)
            {
                if(!HasStreamedTextureDictLoaded("mpleaderboard"))
                {
                    RequestStreamedTextureDict("mpleaderboard", true);
                    while(!HasStreamedTextureDictLoaded("mpleaderboard"))
                    {
                        await Delay(1);
                    }
                }

                if (!HasStreamedTextureDictLoaded("mpsrange"))
                {
                    RequestStreamedTextureDict("mpsrange", true);
                    while (!HasStreamedTextureDictLoaded("mpsrange"))
                    {
                        await Delay(1);
                    }
                }

                SetTimecycleModifier("scanline_cam_cheap");

                float distPer = (distanceCheck / MaxDistance) * 10;
                distPer /= 2;

                if(distPer <= 2.0f)
                {
                    distPer = 2.0f;
                    DrawSprite("mpleaderboard", "leaderboard_audio_3", 0.095f, 0.750f, 0.04f, 0.06f, 0.0f, 0, 255, 0, 255);
                } else if(distPer >= 2.01f && distPer <= 3.49f)
                {
                    DrawSprite("mpleaderboard", "leaderboard_audio_2", 0.095f, 0.750f, 0.04f, 0.06f, 0.0f, 255, 255, 0, 255);
                } else if(distPer >= 3.5f && distPer <= 4.99f)
                {
                    DrawSprite("mpleaderboard", "leaderboard_audio_1", 0.095f, 0.750f, 0.04f, 0.06f, 0.0f, 255, 0, 0, 255);
                } else if(distPer > 5.0f)
                {
                    DrawSprite("mpleaderboard", "leaderboard_audio_mute", 0.095f, 0.750f, 0.04f, 0.06f, 0.0f, 255, 0, 0, 255);
                }

                float fuel = GetVehicleFuelLevel(Entity);
                if(fuel > 75.0f)
                {
                    DrawSprite("mpsrange", "panelback", 0.05f, 0.800f, 0.025f, 0.024f, 0.0f, 0, 255, 0, 255);
                    DrawSprite("mpsrange", "panelback", 0.08f, 0.800f, 0.025f, 0.024f, 0.0f, 0, 255, 0, 255);
                    DrawSprite("mpsrange", "panelback", 0.11f, 0.800f, 0.025f, 0.024f, 0.0f, 0, 255, 0, 255);
                    DrawSprite("mpsrange", "panelback", 0.14f, 0.800f, 0.025f, 0.024f, 0.0f, 0, 255, 0, 255);
                } else if(fuel > 50.0f)
                {
                    DrawSprite("mpsrange", "panelback", 0.05f, 0.800f, 0.025f, 0.024f, 0.0f, 255, 255, 0, 255);
                    DrawSprite("mpsrange", "panelback", 0.08f, 0.800f, 0.025f, 0.024f, 0.0f, 255, 255, 0, 255);
                    DrawSprite("mpsrange", "panelback", 0.11f, 0.800f, 0.025f, 0.024f, 0.0f, 255, 255, 0, 255);
                } else if(fuel > 25.0f)
                {
                    DrawSprite("mpsrange", "panelback", 0.05f, 0.800f, 0.025f, 0.024f, 0.0f, 255, 0, 0, 255);
                    DrawSprite("mpsrange", "panelback", 0.08f, 0.800f, 0.025f, 0.024f, 0.0f, 255, 0, 0, 255);
                }else if(fuel > 0.0f)
                {
                    DrawSprite("mpsrange", "panelback", 0.05f, 0.800f, 0.04f, 0.06f, 0.0f, 255, 0, 0, 255);
                }

                SetTimecycleModifierStrength(distPer);
            }
        }

        private void Charge()
        {
            if (!DoesEntityExist(Entity))
                return;

            Vector3 PlayerPos = GetEntityCoords(PlayerPedId(), true);
            Vector3 CarPos = GetEntityCoords(Entity, true);
            float distanceCheck = GetDistanceBetweenCoords(PlayerPos.X, PlayerPos.Y, PlayerPos.Z, CarPos.X, CarPos.Y, CarPos.Z, true);

            if (distanceCheck <= 1.5)
            {
                TaskPlayAnim(PlayerPedId(), "pickup_object", "pickup_low", 8.0f, -8.0f, -1, 0, 0, false, false, false);
                SetVehicleFuelLevel(Entity, GetVehicleFuelLevel(Entity) + 25.0f);
            }
        }

        private void Start()
        {
            if (DoesEntityExist(Entity))
                return;

            Spawn();

            Tick += new Func<Task>(async () =>
            {
                if(DoesEntityExist(Entity) && DoesEntityExist(Driver))
                {
                    await Delay(5);

                    SetVehicleFuelLevel(Entity, GetVehicleFuelLevel(Entity) - 0.01f);
                    Vector3 PlayerPos = GetEntityCoords(PlayerPedId(), true);
                    Vector3 CarPos = GetEntityCoords(Entity, true);
                    float distanceCheck = GetDistanceBetweenCoords(PlayerPos.X, PlayerPos.Y, PlayerPos.Z, CarPos.X, CarPos.Y, CarPos.Z, true);

                    if (distanceCheck < MaxDistance)
                    {
                        if (!NetworkHasControlOfEntity(Driver))
                        {
                            NetworkRequestControlOfEntity(Driver);
                        }
                        else if (!NetworkHasControlOfEntity(Entity))
                        {
                            NetworkRequestControlOfEntity(Entity);
                        }
                    }
                    else
                    {
                        TaskVehicleTempAction(Driver, Entity, 6, 2500);
                    }
                }
            });
        }

        private async void Spawn()
        {
            await LoadModels(new uint[] { (uint)GetHashKey("rcbandito"), 68070371 });
            Vector3 spawnCoords = (GetEntityCoords(PlayerPedId(), true) + GetEntityForwardVector(PlayerPedId())) * 3.0f;
            float spawnHeading = GetEntityHeading(PlayerPedId());

            Entity = CreateVehicle((uint)GetHashKey("rcbandito"), spawnCoords.X, spawnCoords.Y, spawnCoords.Z, spawnHeading, true, true);

            SetVehicleModKit(Entity, 0);
            SetVehicleMod(Entity, 5, GetRandomIntInRange(0, 20), false);
            SetVehicleModColor_1(Entity, 0, GetRandomIntInRange(1, 160), 0);

            SetVehicleEngineHealth(Entity, 650.0f);
            Vehicle vehicle = new Vehicle(Entity);
            //vehicle.Mods.
              

            while(!DoesEntityExist(Entity))
            {
                await Delay(5);
            }

            Driver = CreatePed(5, 68070371, spawnCoords.X, spawnCoords.Y, spawnCoords.Z, spawnHeading, true, true);

            SetEntityInvincible(Driver, true);
            SetEntityVisible(Driver, false, false);

            FreezeEntityPosition(Driver, true);
            SetPedAlertness(Driver, 0);

            TaskSetBlockingOfNonTemporaryEvents(Driver, true);

            TaskWarpPedIntoVehicle(Driver, Entity, -1);
            SetVehicleDoorsLockedForAllPlayers(Entity, true);

            while (!IsPedInVehicle(Driver, Entity, false))
            {
                await Delay(0);
            }

            SetEntityCoords(Driver, 0, 0, 0, false, false, false, false);
            Attach("place");
        }

        private async void EquipTablet()
        {
            await LoadModels(new uint[] { (uint)GetHashKey("prop_cs_tablet") });
            Vector3 pos = GetEntityCoords(PlayerPedId(), true);
            TabletEntity = CreateObject(GetHashKey("prop_cs_tablet"), pos.X, pos.Y, pos.Z, true, false, false);
            AttachEntityToEntity(TabletEntity, PlayerPedId(), GetPedBoneIndex(PlayerPedId(), 28422), -0.03f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, true, true, false, true, 1, true);

            await LoadModels(new string[] { "anim@cellphone@in_car@ps" });
            TaskPlayAnim(PlayerPedId(), "anim@cellphone@in_car@ps", "cellphone_text_read_base", 3.0f, -8, -1, 63, 0, false, false, false);

            while (DoesEntityExist(TabletEntity))
            {
                await Delay(5);

                if (!IsEntityPlayingAnim(PlayerPedId(), "anim@cellphone@in_car@ps", "cellphone_text_read_base", 3))
                {
                    TaskPlayAnim(PlayerPedId(), "anim@cellphone@in_car@ps", "cellphone_text_read_base", 3.0f, -8, -1, 63, 0, false, false, false);
                }
            }

            ClearPedTasks(PlayerPedId());
        }

        private async void DrawInstructions(float distanceCheck)
        {
            Dictionary<int, Dictionary<string, string>> steeringButtons = new Dictionary<int, Dictionary<string, string>>()
            {
                [0] = new Dictionary<string, string>
                {
                    ["label"] = "Right",
                    ["button"] = "~INPUT_CELLPHONE_RIGHT~"
                },
                [1] = new Dictionary<string, string>
                {
                    ["label"] = "Forward",
                    ["button"] = "~INPUT_CELLPHONE_UP~"
                },
                [2] = new Dictionary<string, string>
                {
                    ["label"] = "Reverse",
                    ["button"] = "~INPUT_CELLPHONE_DOWN~"
                },
                [3] = new Dictionary<string, string>
                {
                    ["label"] = "Left",
                    ["button"] = "~INPUT_CELLPHONE_LEFT~"
                },
                [4] = new Dictionary<string, string>
                {
                    ["label"] = "Scroll Down",
                    ["button"] = "~INPUT_CELLPHONE_SCROLL_FORWARD~"
                },
                [5] = new Dictionary<string, string>
                {
                    ["label"] = "Scroll Up",
                    ["button"] = "~INPUT_CELLPHONE_SCROLL_BACKWARD~"
                }
            };
            Dictionary<int, Dictionary<string, string>> buttonsToDraw = new Dictionary<int, Dictionary<string, string>>();

            if (distanceCheck <= MaxDistance)
            {
                for(int i = 0; i < steeringButtons.Count; i++)
                {
                    buttonsToDraw.Add(i, steeringButtons[i]);
                }
                
                if(distanceCheck <= 1.5)
                {
                    buttonsToDraw.Add(buttonsToDraw.Count, new Dictionary<string, string>
                    {
                        ["label"] = "Pick Up",
                        ["button"] = "~INPUT_CONTEXT~"
                    });
                }
            }

            buttonsToDraw.Add(buttonsToDraw.Count, new Dictionary<string, string>
            {
                ["label"] = "Toggle Camera",
                ["button"] = "~INPUT_DETONATE~"
            });

            int instruction = RequestScaleformMovie("instructional_buttons");
            while (!HasScaleformMovieLoaded(instruction))
            {
                await Delay(0);
            }

            PushScaleformMovieFunction(instruction, "CLEAR_ALL");
            PushScaleformMovieFunction(instruction, "TOGGLE_MOUSE_BUTTONS");
            PushScaleformMovieFunctionParameterBool(false);
            PopScaleformMovieFunctionVoid();

            for (int i = 0; i < buttonsToDraw.Count; i++)
            {
                PushScaleformMovieFunction(instruction, "SET_DATA_SLOT");
                PushScaleformMovieFunctionParameterInt(i);

                PushScaleformMovieMethodParameterButtonName(buttonsToDraw[i]["button"]);
                PushScaleformMovieFunctionParameterString(buttonsToDraw[i]["label"]);
                PopScaleformMovieFunctionVoid();
            }

            PushScaleformMovieFunction(instruction, "DRAW_INSTRUCTIONAL_BUTTONS");
            PushScaleformMovieFunctionParameterInt(-1);
            PopScaleformMovieFunctionVoid();
            DrawScaleformMovieFullscreen(instruction, 255, 255, 255, 255, -1);
        }

        private void HandleKeys(float distanceCheck)
        {
            if(IsControlJustReleased(0, 47))
            {
                if (IsCamRendering(Camera)) ToggleCamera(false);
                else ToggleCamera(true);
            }

            if(distanceCheck <= 1.5f)
            {
                if(IsControlJustReleased(0, 38))
                    Attach("pick");
            }

            if(GetVehicleFuelLevel(Entity) <= 0.0f)
            {
                TaskVehicleTempAction(Driver, Entity, 6, 2500);
                return;
            }

            if(distanceCheck < MaxDistance)
            {
                if (IsControlPressed(0, 172) && !IsControlPressed(0, 173))
                    TaskVehicleTempAction(Driver, Entity, 9, 1);

                if (IsControlJustReleased(0, 172) || IsControlJustReleased(0, 173))
                    TaskVehicleTempAction(Driver, Entity, 6, 2500);

                if (IsControlPressed(0, 173) && !IsControlPressed(0, 172))
                    TaskVehicleTempAction(Driver, Entity, 22, 1);

                if (IsControlPressed(0, 174) && IsControlPressed(0, 173))
                    TaskVehicleTempAction(Driver, Entity, 13, 1);

                if (IsControlPressed(0, 175) && IsControlPressed(0, 173))
                    TaskVehicleTempAction(Driver, Entity, 14, 1);

                if (IsControlPressed(0, 172) && IsControlPressed(0, 173))
                    TaskVehicleTempAction(Driver, Entity, 30, 100);

                if (IsControlPressed(0, 174) && IsControlPressed(0, 172))
                    TaskVehicleTempAction(Driver, Entity, 7, 1);

                if (IsControlPressed(0, 175) && IsControlPressed(0, 172))
                    TaskVehicleTempAction(Driver, Entity, 8, 1);

                if(IsControlPressed(0, 174) && !IsControlPressed(0, 172) && !IsControlPressed(0, 173))
                    TaskVehicleTempAction(Driver, Entity, 4, 1);

                if (IsControlPressed(0, 175) && !IsControlPressed(0, 172) && !IsControlPressed(0, 173))
                    TaskVehicleTempAction(Driver, Entity, 5, 1);

                if(IsControlPressed(0, 314))
                {
                    if (GetCamFov(Camera) < 100.0f)
                    {
                        SetCamFov(Camera, GetCamFov(Camera) + 0.3f);
                    }
                }
                if (IsControlPressed(0, 315))
                {
                    if (GetCamFov(Camera) > 5.0f)
                    {
                        SetCamFov(Camera, GetCamFov(Camera) - 0.3f);
                    }
                }
            }
        }

        private async void ToggleCamera(bool state)
        {
            if (!DoesEntityExist(Entity))
                return;

            Vector3 PlayerPos = GetEntityCoords(PlayerPedId(), true);
            Vector3 CarPos = GetEntityCoords(Entity, true);
            double easeTime = 500 * Math.Ceiling(GetDistanceBetweenCoords(PlayerPos.X, PlayerPos.Y, PlayerPos.Z, CarPos.X, CarPos.Y, CarPos.Z, true) / 10);

            if (state)
            {
                if (DoesCamExist(Camera))
                    DestroyCam(Camera, true);

                Camera = CreateCam("DEFAULT_SCRIPTED_CAMERA", true);
                AttachCamToEntity(Camera, Entity, 0.0f, 0.1f, 0.4f, true);

                Tick += new Func<Task>(async () =>
                {
                    while (DoesCamExist(Camera))
                    {
                        await Delay(5);

                        Vector3 rot = GetEntityRotation(Entity, 2);
                        SetCamRot(Camera, rot.X, rot.Y, rot.Z, 2);
                    }
                });

                RenderScriptCams(true, true, (int)easeTime, true, true);
                await Delay((int)easeTime);
                CameraState = true;
            } else
            {
                CameraState = false;
                ClearTimecycleModifier();
                RenderScriptCams(false, true, (int)easeTime, true, false);

                await Delay((int)easeTime);
                DestroyCam(Camera, true);
            }
        }

        private async void Attach(string action)
        {
            if (!DoesEntityExist(Entity))
                return;

            await LoadModels(new string[] { "pickup_object" });

            if(action == "place")
            {
                AttachEntityToEntity(Entity, PlayerPedId(), GetPedBoneIndex(PlayerPedId(), 28422), -0.1f, 0.0f, -0.2f, 70.0f, 0.0f, 270.0f, true, true, false, false, 2, true);
                TaskPlayAnim(PlayerPedId(), "pickup_object", "pickup_low", 8.0f, -8.0f, -1, 0, 0, false, false, false);

                await Delay(800);
                DetachEntity(Entity, false, true);

                Vector3 pos = GetOffsetFromEntityInWorldCoords(PlayerPedId(), 0.0f, 1.0f, 0.5f);
                SetEntityCoords(Entity, pos.X, pos.Y, pos.Z, true, false, false, false);
                PlaceObjectOnGroundProperly(Entity);
                EquipTablet();
            } else if(action == "pick")
            {
                if(DoesCamExist(Camera))
                {
                    ToggleCamera(false);
                }

                DeleteEntity(ref TabletEntity);
                await Delay(100);

                TaskPlayAnim(PlayerPedId(), "pickup_object", "pickup_low", 8.0f, -8.0f, -1, 0, 0, false, false, false);
                await Delay(600);

                AttachEntityToEntity(Entity, PlayerPedId(), GetPedBoneIndex(PlayerPedId(), 28422), -0.1f, 0.0f, -0.2f, 70.0f, 0.0f, 270.0f, true, true, false, false, 2, true);
                await Delay(900);

                DetachEntity(Entity, false, true);

                DeleteVehicle(ref Entity);
                DeleteEntity(ref Driver);

                UnloadModels();

                ESX.TriggerServerCallback("Eternar::RCCar::GET", new Action<dynamic>((data) => {}));
            }
        }

        private async Task LoadModels(uint[] hash)
        {
            foreach (uint i in hash)
            {
                if (!CachedModels.Contains(i))
                    CachedModels.Add(i);

                if (IsModelValid(i))
                {
                    while (!HasModelLoaded(i))
                    {
                        RequestModel(i);
                        await Delay(10);
                    }
                }
            }
        }

        private async Task LoadModels(string[] hash)
        {
            foreach (string i in hash)
            {
                if (!CachedAnims.Contains(i))
                    CachedAnims.Add(i);

                while (!HasAnimDictLoaded(i))
                {
                    RequestAnimDict(i);
                    await Delay(10);
                }
            }
        }

        private void UnloadModels()
        {
            foreach(uint i in CachedModels)
            {
                if(IsModelValid(i))
                {
                    SetModelAsNoLongerNeeded(i);
                }
            }

            foreach(string i in CachedAnims)
            {
                RemoveAnimDict(i);
            }

            CachedModels.Clear();
            CachedAnims.Clear();
        }
    }
}