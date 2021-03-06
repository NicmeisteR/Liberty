﻿/*
* Liberty - http://xboxchaos.com/
*
* Copyright (C) 2011 XboxChaos
* Copyright (C) 2011 ThunderWaffle/AMD
* Copyright (C) 2011 Xeraxic
*
* Liberty is free software; you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published
* by the Free Software Foundation; either version 2 of the License,
* or (at your option) any later version.
*
* Liberty is distributed in the hope that it will be useful, but
* WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
* General Public License for more details.
*
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Liberty.SaveIO;

namespace Liberty.Halo3ODST
{
    /// <summary>
    /// Provides constants for the various campaign difficulties.
    /// </summary>
    public enum Difficulty
    {
        Easy,
        Normal,
        Heroic,
        Legendary
    } 

    /// <summary>
    /// Provides properties and methods for managing Halo 3 campaign saves.
    /// </summary>
    public class CampaignSave : ICampaignSave
    {
        /// <summary>
        /// Creates a new Halo3 CampaignSave based off the data in a mmiof.bmf file
        /// </summary>
        /// <param name="path">The path to the mmiof.bmf file to read from.</param>
        public CampaignSave(string path)
        {
            Stream fileStream = File.OpenRead(path);
            ReadFromStream(fileStream);
            fileStream.Close();
        }

        /// <summary>
        /// Creates a new Halo3 CampaignSave based off the data in a mmiof.bmf stream
        /// </summary>
        /// <param name="stream">The strea to read from.</param>
        public CampaignSave(Stream stream)
        {
            ReadFromStream(stream);
        }

        /// <summary>
        /// Writes any changes made back to the original saves.cfg file.
        /// </summary>
        /// <param name="path">The path to the original file</param>
        public void Update(string path)
        {
            Stream fileStream = File.Open(path, FileMode.Open, FileAccess.ReadWrite);
            WriteToStream(fileStream);
            fileStream.Close();
        }

        /// <summary>
        /// Writes any changes made back to the original stream.
        /// </summary>
        /// <param name="stream">The original stream</param>
        public void Update(Stream stream)
        {
            WriteToStream(stream);
        }

        /// <summary>
        /// Read data from the mmiof.bmf file
        /// </summary>
        /// <param name="stream">Stream of the mmiof.bmf file.</param>
        private void ReadFromStream(Stream stream)
        {
            SaveIO.SaveReader reader = new SaveReader(stream);

            // Read save header
            _saveHeader.ReadFrom(reader);

            // Read the object list
            reader.SeekTo((long)TableOffset.Object);
            _objectList = new ObjectList(reader);

            // Read player info
            reader.SeekTo((long)TableOffset.Players);
            _player = new Player(reader, _objectList);
        }

        /// <summary>
        /// Write data to the mmiof.bmf file
        /// </summary>
        /// <param name="stream">Stream of the mmiof.bmf file.</param>
        private void WriteToStream(Stream stream)
        {
            SaveIO.SaveWriter writer = new SaveWriter(stream);

            // Write Save Header
            _saveHeader.WriteTo(writer);
            writer.SeekTo(0);

            // Write the object list
            _objectList.Update(writer);
            writer.SeekTo(0);

            // Resign the Save
            _saveHeader.Resign(writer, stream);
        }

        #region Declarations
        /// <summary>
        /// The Saves Header
        /// </summary>
        public SaveHeader Header
        {
            get { return _saveHeader; }
            set { _saveHeader = value; }
        }

        /// <summary>
        /// The list of objects loaded in the save.
        /// </summary>
        public ObjectList Objects
        {
            get { return _objectList; }
        }

        /// <summary>
        /// The player's biped.
        /// </summary>
        public BipedObject PlayerBiped
        {
            get { return _player.Biped; }
        }

        /// <summary>
        /// The player's information from the Player Table
        /// </summary>
        public Player Player
        {
            get { return _player; }
        }
        #endregion

        private Player _player;
        private ObjectList _objectList;
        private SaveHeader _saveHeader;
    }
}
