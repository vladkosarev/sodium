﻿using PetrolPump.Chapter4.Section4;
using Sodium;

namespace PetrolPump.Chapter4.Section6
{
    public class AccumulatePulsesPump : IPump
    {
        public Outputs Create(Inputs inputs)
        {
            LifeCycle lc = new LifeCycle(inputs.SNozzle1,
                inputs.SNozzle2,
                inputs.SNozzle3);
            Cell<double> litersDelivered =
                Accumulate(lc.SStart.Map(_ => Unit.Value),
                    inputs.SFuelPulses,
                    inputs.Calibration);
            return new Outputs()
                .SetDelivery(lc.FillActive.Map(
                    m =>
                        m.Equals(Maybe.Just(Fuel.One)) ? Delivery.Fast1 :
                            m.Equals(Maybe.Just(Fuel.Two)) ? Delivery.Fast2 :
                                m.Equals(Maybe.Just(Fuel.Three)) ? Delivery.Fast3 :
                                    Delivery.Off))
                .SetSaleQuantityLcd(litersDelivered.Map(Formatters.FormatSaleQuantity));
        }

        public static Cell<double> Accumulate(
            Stream<Unit> sClearAccumulator,
            Stream<int> sPulses,
            Cell<double> calibration)
        {
            CellLoop<int> total = new CellLoop<int>();
            total.Loop(sClearAccumulator.Map(u => 0)
                .OrElse(sPulses.Snapshot(total, (pulsesLocal, totalLocal) => pulsesLocal + totalLocal))
                .Hold(0));
            return total.Lift(
                calibration,
                (totalLocal, calibrationLocal) => totalLocal * calibrationLocal);
        }
    }
}