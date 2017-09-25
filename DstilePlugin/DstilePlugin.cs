//----------------------------------------------------------------------------
// NAME: Dstile Plugin
// VERSION: 0.1
// DESCRIPTION: Allows Drag and Drop Import of Imagery
// DEVELOPER: Tisham Dhar
// WEBSITE: http:\\www.apogee.com.au
// REFERENCES: 
//----------------------------------------------------------------------------
//
// Plugin was developed by Apogee Imaging International
// This file is in the Public Domain, and comes with no warranty. 

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

//WorldWind support classes
using WorldWind.Renderable;

namespace DstileGUI
{
    public class DstilePlugin : WorldWind.PluginEngine.Plugin
    {
        private DstileFrontEnd frontend;
        private MenuItem tempMenu = new MenuItem();			// Temp menu item for storing file MenuItems
        private RenderableObjectList dstileLayers;

        #region Accesor Methods
        public RenderableObjectList DstileLayers
        {
            get{return dstileLayers;}
        }
        #endregion

        public override void Load()
        {
            // Setup Drag&Drop functionality
            m_Application.WorldWindow.DragEnter += new DragEventHandler(WorldWindow_DragEnter);
            m_Application.WorldWindow.DragDrop += new DragEventHandler(WorldWindow_DragDrop);
            
            // Add a menu item to the File menu and the Help menu
            MenuItem loadMenuItem = new MenuItem();
            loadMenuItem.Text = "Load Image File...";
            loadMenuItem.Click += new EventHandler(loadMenu_Click);
            int mergeOrder = 0;
            foreach (MenuItem menuItem in m_Application.MainMenu.MenuItems)
            {
                if (menuItem.Text.Replace("&", "") == "File")
                {
                    foreach (MenuItem subMenuItem in menuItem.MenuItems)
                    {
                        subMenuItem.MergeOrder = mergeOrder;

                        if (subMenuItem.Text == "-")
                            mergeOrder = 2;		// Everything after this should come after our new items
                    }

                    tempMenu.Text = menuItem.Text;
                    tempMenu.MergeOrder = 1;	// MergeOrder 1 will have 0 before it and 2 after it
                    tempMenu.MenuItems.Add(loadMenuItem);
                    tempMenu.MenuItems.Add(new MenuItem("-"));
                    menuItem.MergeMenu(tempMenu);
                }
            }
            //TODO: Add Place Holder Dstile Layer
            dstileLayers = new RenderableObjectList("Dstile Layers");

            //Create GUI and keep in the background
            frontend = new DstileFrontEnd(this);
            frontend.Visible = false;
            
            //TODO: Add any existing Dstile based layers
            ParentApplication.WorldWindow.CurrentWorld.RenderableObjects.Add(dstileLayers);
            //from the Master XML
        }

        private void loadMenu_Click(object sender, EventArgs e)
        {
            showFrontEnd();
        }
        
        private void showFrontEnd()
        {
            if (frontend == null)
            {
                frontend = new DstileFrontEnd(this);
            }
            if (frontend.Visible == false)
            {
                frontend.Visible = true;
            }
        }
        

        public override void Unload()
        {
            
            //Dispose GUI
            if(frontend != null)
                frontend.Dispose();
            frontend = null;

            // Remove the menu items
            foreach (MenuItem menuItem in m_Application.MainMenu.MenuItems)
            {
                if (menuItem.Text.Replace("&", "") == "File")
                {
                    foreach (MenuItem subMenuItem in menuItem.MenuItems)
                    {
                        if (subMenuItem.Text == tempMenu.MenuItems[0].Text)
                        {
                            menuItem.MenuItems.RemoveAt(subMenuItem.Index + 1);
                            menuItem.MenuItems.RemoveAt(subMenuItem.Index);
                            break;
                        }
                    }
                }
            }
            // Disable Drag&Drop functionality
            this.Application.WorldWindow.DragEnter -= new DragEventHandler(WorldWindow_DragEnter);
            this.Application.WorldWindow.DragDrop -= new DragEventHandler(WorldWindow_DragDrop);

            
            tempMenu.MenuItems.Clear();

            //TODO: Save if needed any present DSTile layers to
            //master xml
            //TODO: Remove Dstile added layers from Layer Manager
            ParentApplication.WorldWindow.CurrentWorld.RenderableObjects.Remove(dstileLayers);
            dstileLayers.Dispose();
            dstileLayers = null;


            base.Unload();
        }

        #region Drag&Drop handling methods
        /// <summary>
        /// Checks if the object being dropped is a kml or kmz file
        /// </summary>
        private void WorldWindow_DragEnter(object sender, DragEventArgs e)
        {
            if (DragDropIsValid(e))
                e.Effect = DragDropEffects.All;
        }

        /// <summary>
        /// Handles dropping of a kml/kmz file (by loading that file)
        /// </summary>
        private void WorldWindow_DragDrop(object sender, DragEventArgs e)
        {
            if (DragDropIsValid(e))
            {
                // transfer the filenames to a string array
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0 && File.Exists(files[0]))
                {
                    showFrontEnd();
                    frontend.locateData(files[0]);
                }
                    
            }
        }

        /// <summary>
        /// Determines if this plugin can handle the dropped item
        /// </summary>
        private static bool DragDropIsValid(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
            {
                if (((string[])e.Data.GetData(DataFormats.FileDrop)).Length == 1)
                {
                    string extension = Path.GetExtension(((string[])e.Data.GetData(DataFormats.FileDrop))[0]).ToLower(CultureInfo.InvariantCulture);
                    if ((extension == ".tiff") || (extension == ".tif") || (extension == ".img") || (extension == ".jpg") || (extension == ".sid"))
                        return true;
                }
            }
            return false;
        }
        #endregion

    }
}