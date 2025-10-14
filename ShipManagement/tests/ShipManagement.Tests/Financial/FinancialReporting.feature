Feature: Financial reporting
  In order to track spending against budget
  As a fleet accountant
  I want to retrieve financial report data through the API

  Scenario: Valid financial report detail request returns report items
    Given the financial report request targets ship id 1 for June 2023
    When the financial report detail is requested
    Then the response status should be OK
    And the financial report should contain 1 item

  Scenario: Valid financial report summary request returns report items
    Given the financial report request targets ship id 1 for June 2023
    When the financial report summary is requested
    Then the response status should be OK
    And the financial report should contain 1 item

  Scenario: Invalid financial report summary request returns a bad request response
    Given the financial report request targets ship id 1 for month 13 of 2023
    When the financial report summary is requested
    Then the response status should be BadRequest

  Scenario: Financial report detail failure returns an internal server error
    Given the financial report request targets ship id 1 for June 2023
    And the financial detail service fails
    When the financial report detail is requested
    Then the response status should be InternalServerError

  Scenario: Financial report summary failure returns an internal server error
    Given the financial report request targets ship id 1 for month 6 of 2023
    And the financial summary service fails
    When the financial report summary is requested
    Then the response status should be InternalServerError
