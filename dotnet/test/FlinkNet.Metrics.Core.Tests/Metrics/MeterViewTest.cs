/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using FlinkNet.Metrics;
using Xunit;

namespace FlinkNet.Tests.Metrics;

/// <summary>Tests for the <see cref="MeterView"/>.</summary>
public class MeterViewTest
{
    [Fact]
    public void TestGetCount()
    {
        ICounter c = new SimpleCounter();
        c.Inc(5);
        IMeter m = new MeterView(c);

        Assert.Equal(5, m.Count);
    }

    [Fact]
    public void TestMarkEvent()
    {
        ICounter c = new SimpleCounter();
        IMeter m = new MeterView(c);

        Assert.Equal(0, m.Count);
        m.MarkEvent();
        Assert.Equal(1, m.Count);
        m.MarkEvent(2);
        Assert.Equal(3, m.Count);
    }

    [Fact]
    public void TestGetRate()
    {
        ICounter c = new SimpleCounter();
        var m = new MeterView(c);

        // values = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]
        for (int x = 0; x < 12; x++)
        {
            m.MarkEvent(10);
            m.Update();
        }
        // values = [0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120]
        Assert.Equal(2.0, m.Rate, 1); // (120 - 0) / 60

        for (int x = 0; x < 12; x++)
        {
            m.MarkEvent(10);
            m.Update();
        }
        // values = [130, 140, 150, 160, 170, 180, 190, 200, 210, 220, 230, 240, 120]
        Assert.Equal(2.0, m.Rate, 1); // (240 - 120) / 60

        for (int x = 0; x < 6; x++)
        {
            m.MarkEvent(20);
            m.Update();
        }
        // values = [280, 300, 320, 340, 360, 180, 190, 200, 210, 220, 230, 240, 260]
        Assert.Equal(3.0, m.Rate, 1); // (360 - 180) / 60

        for (int x = 0; x < 6; x++)
        {
            m.MarkEvent(20);
            m.Update();
        }
        // values = [280, 300, 320, 340, 360, 380, 400, 420, 440, 460, 480, 240, 260]
        Assert.Equal(4.0, m.Rate, 1); // (480 - 240) / 60

        for (int x = 0; x < 6; x++)
        {
            m.Update();
        }
        // values = [480, 480, 480, 480, 360, 380, 400, 420, 440, 460, 480, 480, 480]
        Assert.Equal(2.0, m.Rate, 1); // (480 - 360) / 60

        for (int x = 0; x < 6; x++)
        {
            m.Update();
        }
        // values = [480, 480, 480, 480, 480, 480, 480, 480, 480, 480, 480, 480, 480]
        Assert.Equal(0.0, m.Rate, 1); // (480 - 480) / 60
    }

    [Fact]
    public void TestTimeSpanBelowUpdateIntervalIsClamped()
    {
        var m = new MeterView(1);
        m.MarkEvent(10);
        m.Update();
        // clamped to one update interval (5s): rate = 10 / 5
        Assert.Equal(2.0, m.Rate, 1);
    }

    [Fact]
    public void TestGaugeBackedMeter()
    {
        long value = 0;
        MeterView m = MeterView.ForGauge(new FunctionGauge(() => value));

        value = 120;
        for (int x = 0; x < 13; x++)
        {
            m.Update();
        }
        Assert.Equal(120, m.Count);
        Assert.Throws<NotSupportedException>(() => m.MarkEvent());
    }

    private sealed class FunctionGauge(Func<long> supplier) : IGauge<long>
    {
        public long GetValue() => supplier();
    }
}
