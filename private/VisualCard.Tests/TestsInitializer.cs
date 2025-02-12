//
// VisualCard  Copyright (C) 2021-2025  Aptivi
//
// This file is part of VisualCard
//
// VisualCard is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// VisualCard is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//

using Microsoft.VisualStudio.TestTools.UnitTesting;
using VisualCard.Common.Diagnostics;

namespace VisualCard.Tests
{
    [TestClass]
    public static class TestsInitializer
    {
        [AssemblyInitialize]
        public static void Setup(TestContext ctx)
        {
            ctx.WriteLine("Enabling VC logger...");
            LoggingTools.EnableLogging = true;
        }
    }
}
