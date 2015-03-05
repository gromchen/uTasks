﻿using JetBrains.Annotations;
using UnityEngine;

namespace uTasks.Tests
{
    public class ConstantRotation : MonoBehaviour
    {
        [SerializeField] private float _speed = 200;

        [UsedImplicitly]
        private void Update()
        {
            transform.Rotate(transform.up*_speed*Time.deltaTime);
        }
    }
}