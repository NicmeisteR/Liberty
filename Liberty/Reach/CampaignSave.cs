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
using System.Linq;
using System.Text;
using System.IO;

namespace Liberty.Reach
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
    /// Provides properties and methods for managing Halo: Reach campaign saves.
    /// </summary>
    public partial class CampaignSave : ICampaignSave
    {
        /// <summary>
        /// Constructs a new CampaignSave object based off of information in an mmiof.bmf file.
        /// </summary>
        /// <param name="mmiofBmfPath">The path to the mmiof.bmf file to load.</param>
        /// <exception cref="ArgumentException">Thrown if the file is an invalid mmiof.bmf file.</exception>
        public CampaignSave(string mmiofBmfPath)
        {
            FileStream stream = new FileStream(mmiofBmfPath, FileMode.Open, FileAccess.Read);

            try
            {
                Verify(stream);
                Process(stream);
            }
            finally
            {
                stream.Close();
            }
        }

        /// <summary>
        /// Constructs a new CampaignSave object from a Stream.
        /// </summary>
        /// <param name="stream">The stream to read from</param>
        /// <exception cref="ArgumentException">Thrown if the save data is invalid.</exception>
        public CampaignSave(Stream stream)
        {
            if (!stream.CanRead || !stream.CanSeek)
                throw new ArgumentException("Campaign save streams must be readable and seekable");

            Verify(stream);
            Process(stream);
        }

        /// <summary>
        /// Updates and resigns the save data, writing any changes back to a stream.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if the stream cannot be written to or does not support seeking</exception>
        public void Update(Stream stream)
        {
            if (!stream.CanWrite || !stream.CanSeek)
                throw new ArgumentException("Campaign save output streams must be writable and seekable");

            SaveIO.SaveWriter writer = new SaveIO.SaveWriter(stream);

            // Update each object
            Player.ResolvePlayerRefs();
            foreach (GameObject obj in _objects)
            {
                if (obj != null)
                    obj.Update(writer);
            }
            _objectChunk.Update(writer);

            // Update player information
            Player.Update(writer);

            // Checkpoint message
            writer.Seek(0x9DF9E0, SeekOrigin.Begin);
            writer.WriteUTF16(_checkpointMsg);

            // Resign the file
            Resign(stream);
        }

        /// <summary>
        /// Updates and resigns the save data, writing any changes back to a mmiof.bmf file.
        /// </summary>
        /// <param name="path">The mmiof.bmf file to write changes back to.</param>
        public void Update(string path)
        {
            FileStream stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
            Update(stream);
            stream.Close();
        }

        /// <summary>
        /// The ID of the .map file that corresponds to the save.
        /// </summary>
        public string Map
        {
            get { return _mapName; }
        }

        /// <summary>
        /// The difficulty that the player was on.
        /// </summary>
        /// <seealso cref="Difficulty"/>
        public Difficulty Difficulty
        {
            get { return _difficulty; }
        }

        /// <summary>
        /// The player's gamertag.
        /// </summary>
        public string Gamertag
        {
            get { return _gamertag; }
        }

        /// <summary>
        /// The player's service tag.
        /// </summary>
        public string ServiceTag
        {
            get { return _serviceTag; }
        }

        /// <summary>
        /// Returns a Player object containing info about player 1.
        /// </summary>
        public GamePlayer Player
        {
            get { return _player; }
        }

        /// <summary>
        /// Returns a bitfield that specifies which skulls are active.
        /// Not much is known about the order of the bits.
        /// </summary>
        public uint Skulls
        {
            get { return _skulls; }
        }

        /// <summary>
        /// A message that should be shown once the game is started.
        /// This defaults to "Checkpoint...Done"
        /// </summary>
        public string Message
        {
            get { return _checkpointMsg; }
            set { _checkpointMsg = value; }
        }

        /// <summary>
        /// Checks the general validity of the save before processing it.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if the save data is invalid.</exception>
        private static void Verify(Stream stream)
        {
            // Check the file size
            if (stream.Length != 0xA70000)
                throw new ArgumentException("The file format is invalid: incorrect file size\r\nExpected 0xA70000 but got 0x" + stream.Length.ToString("X"));

            // Check that the magic number is 0x60CCCCB2
            byte[] header = new byte[4];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(header, 0, 4);
            if (header[0] != 0x60 || header[1] != 0xCC || header[2] != 0xCC || header[3] != 0xB2)
                throw new ArgumentException("The file format is invalid: bad header\r\nShould be 60 CC CC B2");
        }

        /// <summary>
        /// Resigns the save data so that it can be read by the game.
        /// </summary>
        /// <seealso cref="Security.SaveSHA1"/>
        private void Resign(Stream stream)
        {
            Security.SaveSHA1 hasher = new Security.SaveSHA1();

            // Load the whole stream into memory
            MemoryStream memoryStream = new MemoryStream((int)stream.Length);
            memoryStream.SetLength(stream.Length);
            stream.Position = 0;
            stream.Read(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);

            // Hash the contents
            memoryStream.Position = 0x1E708;
            memoryStream.Write(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 0, 20);
            hasher.TransformFinalBlock(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);

            // Write the new digest
            stream.Position = 0x1E708;
            stream.Write(hasher.Hash, 0, 20);
        }

        /// <summary>
        /// Reads the data stored in the file header and some of the chunk data.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if the file format is invalid.</exception>
        /// <seealso cref="Verify"/>
        /// <seealso cref="ReadObjects"/>
        private void Process(Stream stream)
        {
            Liberty.SaveIO.SaveReader reader = new Liberty.SaveIO.SaveReader(stream);

            // Map name
            reader.Seek(8, SeekOrigin.Begin);
            _mapName = reader.ReadAscii();

            // Difficulty
            reader.Seek(0xFAFF, SeekOrigin.Begin);
            _difficulty = (Difficulty)reader.ReadByte();

            // Skulls
            reader.Seek(0xFB0C, SeekOrigin.Begin);
            _skulls = reader.ReadUInt32();

            // Gamertag
            reader.Seek(0x1D668, SeekOrigin.Begin);
            _gamertag = reader.ReadUTF16();

            // Service tag
            reader.Seek(0x1D6AC, SeekOrigin.Begin);
            _serviceTag = reader.ReadUTF16();

            // Message
            reader.Seek(0x9DF9E0, SeekOrigin.Begin);
            _checkpointMsg = reader.ReadUTF16();

            // Read objects
            Chunk objectChunk = new Chunk(reader, ChunkOffset.Object);
            if (objectChunk.Name != "object")
                throw new ArgumentException("The file format is invalid: the \"object\" chunk is missing or is at the wrong offset");
            ReadObjects(reader, objectChunk);

            // Read players
            Chunk playersChunk = new Chunk(reader, ChunkOffset.Players);
            if (playersChunk.Name != "players")
                throw new ArgumentException("The file format is invalid: the \"players\" chunk is missing or is at the wrong offset");
            ReadPlayers(reader, playersChunk);
        }

        private string _mapName;
        private Difficulty _difficulty;
        private string _gamertag;
        private string _serviceTag;
        private GamePlayer _player = null;
        private uint _skulls;
        private string _checkpointMsg;
    }
}