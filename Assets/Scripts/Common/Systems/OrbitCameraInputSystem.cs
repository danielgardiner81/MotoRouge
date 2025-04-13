using Common.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Common.Systems
{
    public partial class OrbitCameraInputSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var isDragging = Input.GetMouseButton(0);
            var dragDelta = float2.zero;
            var zoomDelta = Input.mouseScrollDelta.y;

            if (isDragging)
            {
                dragDelta = new float2(
                    Input.GetAxis("Mouse X"),
                    Input.GetAxis("Mouse Y")
                );
            }

            foreach (var input in SystemAPI.Query<RefRW<OrbitCameraInput>>())
            {
                input.ValueRW = new OrbitCameraInput
                {
                    IsDragging = isDragging,
                    DragDelta = dragDelta,
                    ZoomDelta = zoomDelta
                };
            }
        }
    }
}