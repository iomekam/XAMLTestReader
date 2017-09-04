//  Copyright (c) Microsoft Corporation.  All rights reserved.
#pragma once
#include <Versioning.h>

namespace Windows { namespace UI { namespace Xaml { namespace Tests {
    namespace XamlIslands {
        class XamlIslandsTests : public WEX::TestClass<XamlIslandsTests>
        {
        public:
            BEGIN_TEST_CLASS(XamlIslandsTests)
                TEST_CLASS_PROPERTY(L"Owner", L"iomekam")
                TEST_CLASS_PROPERTY(L"BinaryUnderTest", L"Windows.UI.Xaml.dll")
                TEST_CLASS_PROPERTY(L"Classification", L"Integration")
                TEST_CLASS_PROPERTY(L"RunAs", L"UAP")
                TEST_CLASS_PROPERTY(L"UAP:AppXManifest", WINDOWS_VERSION_CURRENT)
                TEST_CLASS_PROPERTY(L"IsolationLevel", L"Class") // TODO: 11124248 Can we run without isolation?
                TEST_CLASS_PROPERTY(L"Platform", L"Desktop")
                TEST_CLASS_PROPERTY(L"EnabledOnOneCore", L"TRUE")
                TEST_CLASS_PROPERTY(L"RunFixtureAs", L"System") // To make sure velocity feature gets cleared in case of a crash
            END_TEST_CLASS()

            TEST_CLASS_SETUP(ClassSetup)
            TEST_CLASS_CLEANUP(ClassCleanup)

            TEST_METHOD_SETUP(TestSetup)
            TEST_METHOD_CLEANUP(TestCleanup)

            BEGIN_TEST_METHOD(SmokeTest)
                TEST_METHOD_PROPERTY(L"Description", L"Verifies we can instantiate multiple islands")
            END_TEST_METHOD()

            BEGIN_TEST_METHOD(VerifyButtonClickEventHandlers)
                TEST_METHOD_PROPERTY(L"Description", L"Verifies we can use the mouse to click a button in multiple islands")
                TEST_METHOD_PROPERTY(L"EnabledOnOneCore", L"FALSE") // TODO: For some reason the click isn't getting through on OneCore
            END_TEST_METHOD()

            BEGIN_TEST_METHOD(VerifyTextBox)
                TEST_METHOD_PROPERTY(L"Description", L"Verifies a TextBox can receive focus and input")
            END_TEST_METHOD()

            BEGIN_TEST_METHOD(MouseCaptureOnButton)
                TEST_METHOD_PROPERTY(L"Description", L"Verifies mouse capture works right on a button")
            END_TEST_METHOD()

            BEGIN_TEST_METHOD(ValidateToolTips)
                TEST_METHOD_PROPERTY(L"Description", L"ToolTips should appear on the XamlIsland's popup root, not the global one")
            END_TEST_METHOD()

            BEGIN_TEST_METHOD(ValidateMenuFlyout)
                TEST_METHOD_PROPERTY(L"Description", L"MenuFlyout.ShowAt should generate a MenuFlyout at the appropriate place with appropriate input")
            END_TEST_METHOD()

            BEGIN_TEST_METHOD(FindXamlIsland)
                TEST_METHOD_PROPERTY(L"Description", L"Find the XamlIsland using VisualTreeHelper")
            END_TEST_METHOD()

            TEST_METHOD(ThemeChangedWinPerf)
        };
    }
} } } }