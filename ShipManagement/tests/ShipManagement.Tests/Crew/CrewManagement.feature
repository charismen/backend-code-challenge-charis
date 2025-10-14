Feature: Crew Management
  In order to monitor each vessel's crew
  As an operations coordinator
  I want to retrieve crew lists through the API

  Scenario: Valid crew list request returns a paged result
    Given the crew list request has ship id 1, page number 1, and page size 10
    When the crew list is requested
    Then the response status should be OK
    And the crew result should contain 2 members
    And the total crew count should be 2

  Scenario: Invalid crew list request returns a bad request response
    Given the crew list request has ship id 0, page number 1, and page size 10
    When the crew list is requested
    Then the response status should be BadRequest

  Scenario: Crew service failure returns an internal server error
    Given the crew list request has ship id 1, page number 1, and page size 10
    And the crew service fails while retrieving the crew list
    When the crew list is requested
    Then the response status should be InternalServerError
