using BEPUphysics.CollisionShapes;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.Vehicle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoidWar.Gameplay
{
    class VehicleFactory
    {

        public static Vehicle Create(BEPUutilities.Vector3 position)
        {
            var bodies = new List<CompoundShapeEntry>
                {
                    new CompoundShapeEntry(new BoxShape(2.5f, .75f, 4.5f), new BEPUutilities.Vector3(0, 0, 0), 60),
                    new CompoundShapeEntry(new BoxShape(2.5f, .3f, 2f), new BEPUutilities.Vector3(0, .75f / 2 + .3f / 2, .5f), 1)
                };
            var body = new CompoundBody(bodies, 61);
            body.CollisionInformation.LocalPosition = new BEPUutilities.Vector3(0, 0.5f, 0);
            body.Position = position; //At first, just keep it out of the way.
            var Vehicle = new Vehicle(body);

            var localWheelRotation = BEPUutilities.Quaternion.CreateFromAxisAngle(new BEPUutilities.Vector3(0, 0, 1), BEPUutilities.MathHelper.PiOver2);

            /* original values
            float wheelRadius = .375f;
            float wheelWidth = .2f;
            float restLength = 0.325f;
            float wheelStiffness = 2000;
            float wheelDamping = 100;
            float gripFriction = 2.5f;
            float maxForwardForce = 30000;
            float maxBackwardForce = 10000;
            float dynamicBrakingCoeff = 1.5f;
            float staticBrakingCoeff = 2;
            float rollingFrictionCoeff = 0.02f;
            float wheelSlidingDynamicCoeff = 4;
            float wheelSlidingStaticCoeff = 5;

            float wheelLateralPlacement = 1.1f;
            float wheelForeAftPlacement = 1.8f;
            float wheelHeightPlacement = 0.1f;
            */

            float wheelRadius = .375f;
            float wheelWidth = .2f;
            float restLength = 0.325f;
            float wheelStiffness = 2000;
            float wheelDamping = 100;
            float gripFriction = 2.5f;
            float maxForwardForce = 30000;
            float maxBackwardForce = 10000;
            float dynamicBrakingCoeff = 1.5f;
            float staticBrakingCoeff = 2;
            float rollingFrictionCoeff = 0.02f;
            float wheelSlidingDynamicCoeff = 4;
            float wheelSlidingStaticCoeff = 5;

            float wheelLateralPlacement = 1.1f;
            float wheelForeAftPlacement = 1.8f;
            float wheelHeightPlacement = 0.1f;

            //The wheel model used is not aligned initially with how a wheel would normally look, so rotate them.
            BEPUutilities.Matrix wheelGraphicRotation = BEPUutilities.Matrix.CreateFromAxisAngle(BEPUutilities.Vector3.Forward, BEPUutilities.MathHelper.PiOver2);
            Vehicle.AddWheel(new Wheel(
                                 new CylinderCastWheelShape(wheelRadius, wheelWidth, localWheelRotation, wheelGraphicRotation, false),
                                 new WheelSuspension(wheelStiffness, wheelDamping, BEPUutilities.Vector3.Down, restLength, new BEPUutilities.Vector3(-wheelLateralPlacement, -wheelHeightPlacement, wheelForeAftPlacement)),
                                 new WheelDrivingMotor(gripFriction, maxForwardForce, maxBackwardForce),
                                 new WheelBrake(dynamicBrakingCoeff, staticBrakingCoeff, rollingFrictionCoeff),
                                 new WheelSlidingFriction(wheelSlidingDynamicCoeff, wheelSlidingStaticCoeff)));
            Vehicle.AddWheel(new Wheel(
                                 new CylinderCastWheelShape(wheelRadius, wheelWidth, localWheelRotation, wheelGraphicRotation, false),
                                 new WheelSuspension(wheelStiffness, wheelDamping, BEPUutilities.Vector3.Down, restLength, new BEPUutilities.Vector3(-wheelLateralPlacement, -wheelHeightPlacement, -wheelForeAftPlacement)),
                                 new WheelDrivingMotor(gripFriction, maxForwardForce, maxBackwardForce),
                                    new WheelBrake(dynamicBrakingCoeff, staticBrakingCoeff, rollingFrictionCoeff),
                                new WheelSlidingFriction(wheelSlidingDynamicCoeff, wheelSlidingStaticCoeff)));
            Vehicle.AddWheel(new Wheel(
                                 new CylinderCastWheelShape(wheelRadius, wheelWidth, localWheelRotation, wheelGraphicRotation, false),
                                 new WheelSuspension(wheelStiffness, wheelDamping, BEPUutilities.Vector3.Down, restLength, new BEPUutilities.Vector3(wheelLateralPlacement, -wheelHeightPlacement, wheelForeAftPlacement)),
                                 new WheelDrivingMotor(gripFriction, maxForwardForce, maxBackwardForce),
                                    new WheelBrake(dynamicBrakingCoeff, staticBrakingCoeff, rollingFrictionCoeff),
                                 new WheelSlidingFriction(wheelSlidingDynamicCoeff, wheelSlidingStaticCoeff)));
            Vehicle.AddWheel(new Wheel(
                                 new CylinderCastWheelShape(wheelRadius, wheelWidth, localWheelRotation, wheelGraphicRotation, false),
                                 new WheelSuspension(wheelStiffness, wheelDamping, BEPUutilities.Vector3.Down, restLength, new BEPUutilities.Vector3(wheelLateralPlacement, -wheelHeightPlacement, -wheelForeAftPlacement)),
                                 new WheelDrivingMotor(gripFriction, maxForwardForce, maxBackwardForce),
                                   new WheelBrake(dynamicBrakingCoeff, staticBrakingCoeff, rollingFrictionCoeff),
                               new WheelSlidingFriction(wheelSlidingDynamicCoeff, wheelSlidingStaticCoeff)));


            foreach (Wheel wheel in Vehicle.Wheels)
            {
                //This is a cosmetic setting that makes it looks like the car doesn't have antilock brakes.
                wheel.Shape.FreezeWheelsWhileBraking = true;

                //By default, wheels use as many iterations as the space.  By lowering it,
                //performance can be improved at the cost of a little accuracy.
                //However, because the suspension and friction are not really rigid,
                //the lowered accuracy is not so much of a problem.
                wheel.Suspension.SolverSettings.MaximumIterationCount = 1;
                wheel.Brake.SolverSettings.MaximumIterationCount = 1;
                wheel.SlidingFriction.SolverSettings.MaximumIterationCount = 1;
                wheel.DrivingMotor.SolverSettings.MaximumIterationCount = 1;
            }



            return Vehicle;
        }
    }
}
