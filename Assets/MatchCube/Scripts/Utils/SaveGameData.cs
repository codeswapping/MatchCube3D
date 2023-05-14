using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Serialization;

namespace MatchCube.Scripts.Utils
{
    [Serializable]
    public class SaveGameData
    {
        private static SaveGameData _instance;
        public static SaveGameData Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = LoadGame();
                return _instance;
            }
        }
        public static string SaveGameDataKey = "MatchCubeSaveGameData.bin";
        [FormerlySerializedAs("GameData")] public GameData gameData;
        [FormerlySerializedAs("GameDataExists")] public bool gameDataExists = false;

        public static void SaveGame(SaveGameData data)
        {
            data.gameDataExists = true;
            var formatter = new BinaryFormatter();
            var path = Application.persistentDataPath + "/" + SaveGameDataKey;
            var stream = new FileStream(path,FileMode.Create);
            formatter.Serialize(stream,data);
            stream.Close();
            stream.Flush();
        }

        public static SaveGameData LoadGame()
        {
            var path = Application.persistentDataPath + "/" + SaveGameDataKey;
            if (!File.Exists(path)) return new SaveGameData();
            var formatter = new BinaryFormatter();
            var stream = new FileStream(path,FileMode.Open);
            var savegame = (SaveGameData) formatter.Deserialize(stream);
            stream.Close();
            stream.Flush();
            return savegame;
        }
    }

    [Serializable]
    public class GameData
    {
        public BoxData[] AllBoxes;
        public bool IsGameStarted;
        public float CurrentTime;
    }

    [Serializable]
    public class BoxData
    {
        public Vector3 BoxPosition;
        public Quaternion BoxRotation;
        public Vector3 BoxVelocity;
        public Vector3 BoxTorque;
        public bool CanMove;
    }

    [Serializable]
    public class PlayerData
    {
        public string PlayerId;
        public string PlayerName;
    }
}