using System;
using System.Collections.Generic;
using System.Linq;
using MatchCube.Scripts.Utils;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace MatchCube.Scripts.Managers
{
    public class GameManager : MonoBehaviour
    {
        #region Constants

        private const int MaxCount = 5;

        #endregion
        
        #region Public Variables

        public static GameManager Instance;
        [FormerlySerializedAs("BoxPrefab")] public GameObject boxPrefab;
        [FormerlySerializedAs("BoxShatteredEffectPrefab")] public GameObject boxShatteredEffectPrefab;
        public Transform aimLineTransform;
        [FormerlySerializedAs("BoxUpForce")] public float boxUpForce = 50f;
        [FormerlySerializedAs("BoxTorque")] public float boxTorque = 10f;
        public bool setNext = true;
        public float waitToSpawn = 0.5f;
        [FormerlySerializedAs("_generatedNumbers")] public BoxInfo[] generatedNumbers;

        #endregion

        #region Private Variables
        
        private float _currentWait;
        private bool _gameStarted = false;
        private SaveGameData _gameData;
        #endregion

        #region Untiy Methods

        private void Awake()
        {
            if (Instance != null)
            {
                if (Instance != this)
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                Instance = this;
            }
        }

        private void Start()
        {
            _gameData = SaveGameData.Instance;
            if (_gameData.gameDataExists)
            {
                Time.timeScale = 0;
                _currentWait = _gameData.gameData.CurrentTime;
                //Box Generation
                foreach (var box in _gameData.gameData.AllBoxes)
                {
                    var cube = Instantiate(boxPrefab, box.BoxPosition, box.BoxRotation).GetComponent<Cube>();
                    cube.CanMove = box.CanMove;
                    var rb = cube.GetComponent<Rigidbody>();
                    rb.velocity = box.BoxVelocity;
                    rb.angularVelocity = box.BoxTorque;
                }
            }
        }

        private void Update()
        {
            if (!_gameStarted)
            {
                //_cubes.RemoveAll(item => item == null);
                var boxList = GameObject.FindObjectsOfType<Cube>();
                ulong bigNum = 0;
                foreach (var c in boxList)
                {
                    if (c.enteredCube == null || c.Number != c.enteredCube.Number || c.isDestroying) continue;
                    var boxPos = c.transform.position;
                    //Create New Box with higher value
                    var next = c.Number * 2;
                    var cube = Instantiate(boxPrefab, boxPos, Quaternion.identity).GetComponent<Cube>();
                    cube.Number = next;
                    var color = next > generatedNumbers[generatedNumbers.Length - 1].number
                        ? generatedNumbers[generatedNumbers.Length - 1].boxColor
                        : generatedNumbers.First(x => x.number == next).boxColor;
                    cube.GetComponent<Renderer>().material.color = color;
                    bigNum = bigNum < next ? next : bigNum;
                    var rigid = cube.GetComponent<Rigidbody>();
                    rigid.AddTorque(new Vector3(boxTorque, boxTorque, boxTorque),
                        ForceMode.Impulse);
                    rigid.AddForce(new Vector3(boxUpForce, boxUpForce, boxUpForce),
                        ForceMode.Impulse);

                    //Create Effect for Box Shattering
                    var boxEffect = Instantiate(boxShatteredEffectPrefab, boxPos, Quaternion.identity)
                        .GetComponent<ParticleSystem>();
                    var col = boxEffect.colorOverLifetime;
                    col.enabled = true;

                    var grad = new Gradient();
                    grad.SetKeys(new[]
                        {
                            new GradientColorKey(color, 0.0f),
                            new GradientColorKey(color, 1.0f)
                        },
                        new[]
                        {
                            new GradientAlphaKey(1.0f, 0.0f),
                            new GradientAlphaKey(0.0f, 1.0f)
                        });
                    col.color = grad;

                    //Destroying merged cubes
                    c.isDestroying = true;
                    c.enteredCube.isDestroying = true;
                    Destroy(c.enteredCube.gameObject);
                    Destroy(c.gameObject);
                }

                if (bigNum > 512)
                {
                    //Lets celebrate
                }

                if (!setNext) return;
                if (_currentWait <= 0)
                {
                    var cube1 = Instantiate(boxPrefab, new Vector3(0, 1, -2), 
                        Quaternion.identity).GetComponent<Cube>();
                    var ind = Random.Range(0, MaxCount);
                    cube1.Number = generatedNumbers[ind].number;
                    cube1.GetComponent<Renderer>().material.color = generatedNumbers[ind].boxColor;
                    cube1.CanMove = true;
                    setNext = false;
                }
                else
                {
                    _currentWait -= Time.deltaTime;
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    StartGame();
                }
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                Time.timeScale = 0;
                _gameData.gameData.CurrentTime = _currentWait;
                var boxList = GameObject.FindObjectsOfType<Cube>();
                _gameData.gameData.AllBoxes = (from cube in boxList
                    let transform1 = cube.transform
                    select new BoxData
                    {
                        CanMove = cube.CanMove,
                        BoxPosition = transform1.position,
                        BoxRotation = transform1.rotation,
                        BoxTorque = cube.GetComponent<Rigidbody>().angularVelocity,
                        BoxVelocity = cube.GetComponent<Rigidbody>().velocity
                    }).ToArray();
                SaveGameData.SaveGame(_gameData);
            }
        }

        #endregion

        #region Public Methods
        
        public void StartGame()
        {
            var boxList = GameObject.FindObjectsOfType<Cube>();
            foreach (var c in boxList)
            {
                Destroy(c.gameObject);
            }
            _currentWait = 0.0f;
            setNext = true;
        }

        public void ContinueGame()
        {
            _gameStarted = true;
            Time.timeScale = 1;
        }

        public void SetNextTime()
        {
            setNext = true;
            _currentWait = waitToSpawn;
        }
        
        #endregion
        
        [Serializable]
        public struct BoxInfo
        {
            [FormerlySerializedAs("Number")] public ulong number;
            [FormerlySerializedAs("BoxColor")] public Color boxColor;
        }
    }
}
