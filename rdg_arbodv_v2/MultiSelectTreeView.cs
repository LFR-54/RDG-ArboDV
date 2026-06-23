using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RDG_Uploader_GUI
{
    /// <summary>
    /// Un TreeView qui supporte la sélection multiple avec Ctrl+clic et Maj+clic.
    /// </summary>
    public class MultiSelectTreeView : TreeView
    {
        // Liste des nœuds actuellement sélectionnés
        public List<TreeNode> SelectedNodes { get; } = new List<TreeNode>();

        private TreeNode _lastNode;   // Pour SHIFT+clic : le dernier nœud cliqué

        public MultiSelectTreeView()
        {
            // Redessine les nœuds sélectionnés dans la couleur système de sélection
            this.DrawMode = TreeViewDrawMode.OwnerDrawText;
            this.HideSelection = false;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            var hit = this.HitTest(e.Location);
            TreeNode clickedNode = hit.Node;
            if (clickedNode == null) return;

            // Only process selection if the click is on the label, image, state image, or right of the label
            bool isSelectionClick = (hit.Location & (TreeViewHitTestLocations.Label | TreeViewHitTestLocations.Image | TreeViewHitTestLocations.StateImage | TreeViewHitTestLocations.RightOfLabel)) != 0;
            if (!isSelectionClick) return;

            if (e.Button == MouseButtons.Right)
            {
                // Right click: if clickedNode is already selected, keep selection.
                // Otherwise, clear and select only clickedNode.
                if (!SelectedNodes.Contains(clickedNode))
                {
                    ClearAllSelections();
                    SelectNode(clickedNode);
                    _lastNode = clickedNode;
                }
            }
            else
            {
                bool ctrl = (Control.ModifierKeys & Keys.Control) == Keys.Control;
                bool shift = (Control.ModifierKeys & Keys.Shift) == Keys.Shift;

                if (shift && _lastNode != null)
                {
                    // Sélection par plage entre _lastNode et clickedNode
                    SelectNodesInRange(_lastNode, clickedNode);
                }
                else if (ctrl)
                {
                    // Toggle sélection individuelle
                    ToggleNodeSelection(clickedNode);
                    _lastNode = clickedNode;
                }
                else
                {
                    // Sélection simple : désélectionner tout, puis sélectionner clic
                    ClearAllSelections();
                    SelectNode(clickedNode);
                    _lastNode = clickedNode;
                }
            }

            this.Focus();        // Conserver le focus sur le TreeView
            this.Invalidate();   // Redessiner pour mettre à jour l’affichage
        }

        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            string text = e.Node.Text;
            string filePart = text;
            string metaPart = "";

            int parenIndex = text.LastIndexOf(" (");
            if (parenIndex >= 0)
            {
                filePart = text.Substring(0, parenIndex);
                metaPart = text.Substring(parenIndex);
            }

            // Si le nœud est sélectionné dans notre liste, on dessine l’arrière‐plan sélectionné
            if (SelectedNodes.Contains(e.Node))
            {
                e.Graphics.FillRectangle(SystemBrushes.Highlight, e.Bounds);
                if (parenIndex >= 0)
                {
                    // Mesurer la première partie (sans padding)
                    Size filePartSize = TextRenderer.MeasureText(e.Graphics, filePart, this.Font, e.Bounds.Size, TextFormatFlags.NoPadding);
                    
                    // Décalage pour tenir compte du padding par défaut GDI+ du premier dessin
                    int splitX = filePartSize.Width + 3;

                    Rectangle fileBounds = new Rectangle(e.Bounds.X, e.Bounds.Y, splitX, e.Bounds.Height);
                    Rectangle metaBounds = new Rectangle(e.Bounds.X + splitX, e.Bounds.Y, e.Bounds.Width - splitX, e.Bounds.Height);

                    TextRenderer.DrawText(e.Graphics, filePart, this.Font, fileBounds,
                        SystemColors.HighlightText, TextFormatFlags.GlyphOverhangPadding);

                    Color selectedMetaColor = Color.FromArgb(200, 220, 255); // Bleu-gris très clair et doux pour le texte sélectionné secondaire
                    TextRenderer.DrawText(e.Graphics, metaPart, this.Font, metaBounds,
                        selectedMetaColor, TextFormatFlags.GlyphOverhangPadding);
                }
                else
                {
                    TextRenderer.DrawText(e.Graphics, text, this.Font, e.Bounds,
                        SystemColors.HighlightText, TextFormatFlags.GlyphOverhangPadding);
                }
            }
            else
            {
                // Sinon dessin normal. On dessine manuellement pour éviter que le système ne dessine
                // le focus ou l'arrière-plan de sélection si ce nœud est le SelectedNode du TreeView de base.
                using (Brush bgBrush = new SolidBrush(this.BackColor))
                {
                    e.Graphics.FillRectangle(bgBrush, e.Bounds);
                }

                Color fileColor = e.Node.ForeColor != Color.Empty ? e.Node.ForeColor : this.ForeColor;

                if (parenIndex >= 0)
                {
                    // Mesurer la première partie (sans padding)
                    Size filePartSize = TextRenderer.MeasureText(e.Graphics, filePart, this.Font, e.Bounds.Size, TextFormatFlags.NoPadding);
                    
                    // Décalage pour tenir compte du padding par défaut GDI+ du premier dessin
                    int splitX = filePartSize.Width + 3;

                    Rectangle fileBounds = new Rectangle(e.Bounds.X, e.Bounds.Y, splitX, e.Bounds.Height);
                    Rectangle metaBounds = new Rectangle(e.Bounds.X + splitX, e.Bounds.Y, e.Bounds.Width - splitX, e.Bounds.Height);

                    TextRenderer.DrawText(e.Graphics, filePart, this.Font, fileBounds,
                        fileColor, TextFormatFlags.GlyphOverhangPadding);

                    Color metaColor = Color.FromArgb(160, 165, 170); // Gris-ardoise plus clair et discret (réduit l'impact visuel)
                    TextRenderer.DrawText(e.Graphics, metaPart, this.Font, metaBounds,
                        metaColor, TextFormatFlags.GlyphOverhangPadding);
                }
                else
                {
                    TextRenderer.DrawText(e.Graphics, text, this.Font, e.Bounds,
                        fileColor, TextFormatFlags.GlyphOverhangPadding);
                }
            }
        }

        private void SelectNode(TreeNode node)
        {
            if (!SelectedNodes.Contains(node))
                SelectedNodes.Add(node);
        }

        private void ToggleNodeSelection(TreeNode node)
        {
            if (SelectedNodes.Contains(node))
                SelectedNodes.Remove(node);
            else
                SelectedNodes.Add(node);
        }

        private void ClearAllSelections()
        {
            SelectedNodes.Clear();
        }

        private void SelectNodesInRange(TreeNode a, TreeNode b)
        {
            // Sélectionne tous les nœuds entre a et b dans l'ordre de TreeView.Nodes linéarisé
            var all = new List<TreeNode>();
            FlattenNodes(this.Nodes, all);

            int i = all.IndexOf(a), j = all.IndexOf(b);
            if (i < 0 || j < 0) return;
            if (i > j) (i, j) = (j, i);

            ClearAllSelections();
            for (int k = i; k <= j; k++)
                SelectedNodes.Add(all[k]);
        }

        private void FlattenNodes(TreeNodeCollection nodes, List<TreeNode> list)
        {
            foreach (TreeNode n in nodes)
            {
                list.Add(n);
                if (n.Nodes.Count > 0)
                    FlattenNodes(n.Nodes, list);
            }
        }
    }
}
