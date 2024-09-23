// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

// <auto-generated>This file has been auto generated from 'src\OpenTelemetry.SemanticConventions\scripts\templates\registry\SemanticConventionsAttributes.cs.j2' </auto-generated>

#nullable enable

#pragma warning disable CS1570 // XML comment has badly formed XML

namespace OpenTelemetry.SemanticConventions;

/// <summary>
/// Constants for semantic attribute names outlined by the OpenTelemetry specifications.
/// </summary>
public static class TestAttributes
{
    /// <summary>
    /// The fully qualified human readable name of the <a href="https://en.wikipedia.org/wiki/Test_case">test case</a>.
    /// </summary>
    public const string AttributeTestCaseName = "test.case.name";

    /// <summary>
    /// The status of the actual test case result from test execution.
    /// </summary>
    public const string AttributeTestCaseResultStatus = "test.case.result.status";

    /// <summary>
    /// The human readable name of a <a href="https://en.wikipedia.org/wiki/Test_suite">test suite</a>.
    /// </summary>
    public const string AttributeTestSuiteName = "test.suite.name";

    /// <summary>
    /// The status of the test suite run.
    /// </summary>
    public const string AttributeTestSuiteRunStatus = "test.suite.run.status";

    /// <summary>
    /// The status of the actual test case result from test execution.
    /// </summary>
    public static class TestCaseResultStatusValues
    {
        /// <summary>
        /// pass.
        /// </summary>
        public const string Pass = "pass";

        /// <summary>
        /// fail.
        /// </summary>
        public const string Fail = "fail";
    }

    /// <summary>
    /// The status of the test suite run.
    /// </summary>
    public static class TestSuiteRunStatusValues
    {
        /// <summary>
        /// success.
        /// </summary>
        public const string Success = "success";

        /// <summary>
        /// failure.
        /// </summary>
        public const string Failure = "failure";

        /// <summary>
        /// skipped.
        /// </summary>
        public const string Skipped = "skipped";

        /// <summary>
        /// aborted.
        /// </summary>
        public const string Aborted = "aborted";

        /// <summary>
        /// timed_out.
        /// </summary>
        public const string TimedOut = "timed_out";

        /// <summary>
        /// in_progress.
        /// </summary>
        public const string InProgress = "in_progress";
    }
}
