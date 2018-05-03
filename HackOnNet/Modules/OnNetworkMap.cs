using Hacknet;
using HackOnNet.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HackOnNet.Modules
{
    class OnNetworkMap : OnCoreModule
    {
        public static int NODE_SIZE = 26;

        public static float ADMIN_CIRCLE_SCALE = 0.62f;
        public static float PULSE_DECAY = 0.5f;
        public static float PULSE_FREQUENCY = 0.8f;

        private Texture2D circle;
        private Texture2D circleOutline;
        private Texture2D adminCircle;
        private Texture2D nodeCircle;
        private Texture2D adminNodeCircle;
        private Texture2D nodeGlow;
        private Texture2D homeNodeCircle;
        private Texture2D targetNodeCircle;
        private Texture2D assetServerNodeOverlay;

        private string label;
        private bool isNodesInSync = false;

        private Vector2 circleOrigin;

        private float rotation = 0f;
        private float pulseFade = 1f;
        private float pulseTimer = NetworkMap.PULSE_FREQUENCY;

        public ConnectedNodeEffect nodeEffect;
        public ConnectedNodeEffect adminNodeEffect;

        public bool DimNonConnectedNodes = false;

        public NetmapSortingAlgorithm SortingAlgorithm = NetmapSortingAlgorithm.Scatter;

        public List<NodeCircle> nodeList = new List<NodeCircle>();

        public NodeCircle dragging;
        public int drag = 60;


        public OnNetworkMap(Rectangle location, UserScreen screen) : base(location, screen)
		{
        }

        public override void LoadContent()
        {
            this.label = "Network Map : World Wide Web";
            //this.nodeEffect = new ConnectedNodeEffect(this.userScreen);
            //this.adminNodeEffect = new ConnectedNodeEffect(this.userScreen);
            this.circle = TextureBank.load("Circle", this.userScreen.content);
            this.nodeCircle = TextureBank.load("NodeCircle", this.userScreen.content);
            this.adminNodeCircle = TextureBank.load("AdminNodeCircle", this.userScreen.content);
            this.homeNodeCircle = TextureBank.load("HomeNodeCircle", this.userScreen.content);
            this.targetNodeCircle = TextureBank.load("TargetNodeCircle", this.userScreen.content);
            this.assetServerNodeOverlay = TextureBank.load("AssetServerNodeOverlay", this.userScreen.content);
            this.circleOutline = TextureBank.load("CircleOutline", this.userScreen.content);
            this.adminCircle = TextureBank.load("AdminCircle", this.userScreen.content);
            this.nodeGlow = TextureBank.load("RadialGradient", this.userScreen.content);
            this.circleOrigin = new Vector2((float)(this.circleOutline.Width / 2), (float)(this.circleOutline.Height / 2));
        }

        public override void Update(float t)
        {
            this.rotation += t / 2f;
            if (this.pulseFade > 0f)
            {
                this.pulseFade -= t * NetworkMap.PULSE_DECAY;
            }
            else
            {
                this.pulseTimer -= t;
                if (this.pulseTimer <= 0f)
                {
                    this.pulseFade = 1f;
                    this.pulseTimer = NetworkMap.PULSE_FREQUENCY;
                }
            }
            /*for (int i = 0; i < this.nodes.Count; i++)
            {
                if (this.nodes[i].disabled)
                {
                    this.nodes[i].bootupTick(t);
                }
            }*/
        }

        public override void Draw(float t)
        {
            base.Draw(t);
            this.DoGui(t);
        }

        public Vector2 getRandomPosition()
        {
            Vector2 vector;
            Vector2 result;
            for (int i = 0; i < 50; i++)
            {
                vector = this.generatePos();
                if (!this.collides(vector, -1f))
                {
                    result = vector;
                    return result;
                }
            }
            vector = this.generatePos();
            result = vector;
            return result;
        }

        public void DiscoverNode(string ip, bool updateDb)
        {
            Vector2 pos = generatePos();
            foreach (var node in nodeList)
            {
                if(node.ip == ip)
                {
                    return;
                }
            }
            nodeList.Add(new NodeCircle(ip, pos));
            if (updateDb)
                userScreen.Execute("netmap " + ip + " " + pos.X.ToString() + " " + pos.Y.ToString());
        }

        public void DiscoverNode(string ip, Vector2 pos, bool updateDb)
        {
            foreach (var node in nodeList)
            {
                if (node.ip == ip)
                {
                    return;
                }
            }
            nodeList.Add(new NodeCircle(ip, pos));
            if (updateDb)
                userScreen.Execute("netmap " + ip + " " + pos.X.ToString() + " " + pos.Y.ToString());
        }

        private Vector2 generatePos()
        {
            float num = (float)NetworkMap.NODE_SIZE;
            var pos = new Vector2((float)Utils.random.NextDouble(), (float)Utils.random.NextDouble());
            for (int i = 0; i < 5; i++)
            {
                if(collides(pos))
                    pos = new Vector2((float)Utils.random.NextDouble(), (float)Utils.random.NextDouble());
                if (collides(pos))
                {
                    pos.X += 0.1f;
                }
                if (collides(pos))
                {
                    pos.Y += 0.1f;
                }
                    
            }
            return pos;
        }

        public bool collides(Vector2 location, float minSeperation = -1f)
        {
            bool result;

            float num = 0.075f;
            if (minSeperation > 0f)
            {
                num = minSeperation;
            }
            for (int i = 0; i < this.nodeList.Count; i++)
            {
                if (Vector2.Distance(location, this.nodeList[i].position) <= num)
                {
                    result = true;
                    return result;
                }
            }
            result = false;

            return result;
        }

        public void DoGui(float t)
        {
            if (userScreen.netManager.nodesToSync != null)
            {
                if (!isNodesInSync && userScreen.netManager.gotNodes)
                {
                    string[] nodes = userScreen.netManager.nodesToSync.Split(',');
                    foreach (var node in nodes)
                    {
                        string[] ippos = node.Split(new char[] { ':' }, 2);
                        string[] pos = ippos[1].Split(':');
                        userScreen.netMap.DiscoverNode(ippos[0], new Vector2(float.Parse(pos[0]), float.Parse(pos[1])), false);
                    }
                    isNodesInSync = true;
                }
            }

            int num = -1;
            Color color = this.userScreen.highlightColor;

            if(dragging != null)
            {
                if (GuiData.getKeyboadState().IsKeyUp(Microsoft.Xna.Framework.Input.Keys.LeftControl))
                    dragging = null;
                else
                    dragging.position = MousePosToNMap(GuiData.getMousePos());
            }

            lock (this.nodeList)
            {
                for (int i = 0; i < this.nodeList.Count; i++)
                {
                    color = this.userScreen.highlightColor;

                    Vector2 nodeDrawPos2 = this.GetNodeDrawPosDebug(this.nodeList[i].position);

                    if (userScreen.activeSession != null && userScreen.activeSession.ip == nodeList[i].ip)
                        color = Color.White;

                    if (Hacknet.Gui.Button.doButton(2000 + i, this.bounds.X + (int)nodeDrawPos2.X, this.bounds.Y + (int)nodeDrawPos2.Y, NetworkMap.NODE_SIZE, NetworkMap.NODE_SIZE, "", new Color?(color), this.nodeCircle))
                    {
                        if (this.dragging == null)
                        {
                            if(GuiData.getKeyboadState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl))
                            {
                                dragging = nodeList[i];
                            }
                            else
                            {
                                this.userScreen.Execute("connect " + this.nodeList[i].ip);
                            }
                        }
                    }
                    if (GuiData.hot == 2000 + i)
                    {
                        num = i;
                    }
                }
            }

            if (num != -1)
            {
                try
                {
                    int i = num;
                    int num3 = i;
                    Vector2 vector = this.GetNodeDrawPosDebug(this.nodeList[num3].position);
                    Vector2 ttpos = new Vector2((float)(this.bounds.X + (int)vector.X + NetworkMap.NODE_SIZE), (float)(this.bounds.Y + (int)vector.Y));
                    string text = this.nodeList[num3].ip;//getTooltipString();
                    Vector2 textSize = GuiData.tinyfont.MeasureString(text);

                    userScreen.postFXDrawActions = (Action)System.Delegate.Combine(userScreen.postFXDrawActions, new Action(delegate
                    {
                        GuiData.spriteBatch.Draw(Utils.white, new Rectangle((int)ttpos.X, (int)ttpos.Y, (int)textSize.X, (int)textSize.Y), this.userScreen.netmapToolTipBackground);
                        Hacknet.Gui.TextItem.doFontLabel(ttpos, text, GuiData.tinyfont, new Color?(this.userScreen.netmapToolTipColor), 3.40282347E+38f, 3.40282347E+38f, false);
                    }));
                }
                catch (System.Exception ex)
                {
                    DebugLog.add(ex.ToString());
                }
            }
        }

        private Vector2 MousePosToNMap(Vector2 vector)
        {
            var nodeSize = NetworkMap.NODE_SIZE;

            var vector2 = new Vector2(vector.X - this.bounds.X - nodeSize / 2, vector.Y - this.bounds.Y - nodeSize / 2);
            var result = new Vector2(
                ((vector2.X - nodeSize / 4f) / (this.bounds.Width - nodeSize - 6)) , 
                ((vector2.Y - nodeSize / 4f) / (this.bounds.Height - nodeSize - 6)) 
                );
            return Utils.Clamp(result, 0f, 1f);
        }

        public Vector2 GetNodeDrawPosDebug(Vector2 nodeLocation)
        {
            int num = 3;
            nodeLocation = Utils.Clamp(nodeLocation, 0f, 1f);
            float num2 = (float)this.bounds.Width - (float)NetworkMap.NODE_SIZE * 1f;
            float num3 = (float)this.bounds.Height - (float)NetworkMap.NODE_SIZE * 1f;
            num2 -= (float)(2 * num);
            num3 -= (float)(2 * num);
            Vector2 result = new Vector2(nodeLocation.X * num2 + (float)NetworkMap.NODE_SIZE / 4f, nodeLocation.Y * num3 + (float)NetworkMap.NODE_SIZE / 4f);
            return result;
        }

        /*public Vector2 GetNodeDrawPos(NodeCircle node)
        {
            int num = 3;
            Vector2 vector = Utils.Clamp(node.position, 0f, 1f);
            float num2 = (float)this.bounds.Width - (float)NetworkMap.NODE_SIZE * 1f;
            float num3 = (float)this.bounds.Height - (float)NetworkMap.NODE_SIZE * 1f;
            num2 -= (float)(2 * num);
            num3 -= (float)(2 * num);
            return nodePosition + new Vector2((float)NetworkMap.NODE_SIZE / 4f);
        }*/

        public void drawLine(Vector2 origin, Vector2 dest, Vector2 offset)
        {
            Vector2 value = new Vector2((float)(NetworkMap.NODE_SIZE / 2));
            origin += value;
            dest += value;
            float y = Vector2.Distance(origin, dest);
            float num = (float)System.Math.Atan2((double)(dest.Y - origin.Y), (double)(dest.X - origin.X));
            num += 4.712389f;
            this.spriteBatch.Draw(Utils.white, origin + offset, null, this.userScreen.outlineColor, num, Vector2.Zero, new Vector2(1f, y), SpriteEffects.None, 0.5f);
        }
    }
}
