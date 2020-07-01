#!/usr/bin/env node
import 'source-map-support/register';
import * as cdk from '@aws-cdk/core';

import { VpcStack } from '../lib/vpc-stack';
import { FanoutStack } from '../lib/fanout-stack';

const app = new cdk.App();
new VpcStack(app, 'VpcStack');
new FanoutStack(app, 'FanoutStack');
