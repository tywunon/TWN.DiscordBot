﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TWN.DiscordBot.Interfaces;
public interface IHealthCheckProvider<T>
{
  Task<T> HealthCheckAsync(CancellationToken cancellationToken);
}
