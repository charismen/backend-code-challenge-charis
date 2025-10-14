Feature: Ship management
  In order to administrate the fleet inventory
  As a fleet manager
  I want to access ship information through the API

  Scenario: Retrieving all ships returns the available vessels
    Given ships SHIP1 and SHIP2 exist
    When all ships are requested
    Then the response status should be OK
    And the ship list should contain 2 ships

  Scenario: Retrieving a ship by code returns the matching vessel
    Given ship SHIP1 exists
    When ship SHIP1 is requested by code
    Then the response status should be OK
    And the returned ship should have code SHIP1

  Scenario: Retrieving a ship with an unknown code returns not found
    Given no ship exists with code INVALID
    When ship INVALID is requested by code
    Then the response status should be NotFound

  Scenario: Creating a ship with valid data returns the new id
    Given a new ship with code SHIP3 and year built 2022
    When the ship is created
    Then the response status should be Created
    And the payload should contain the new ship id 3

  Scenario: Retrieving all ships when the service fails returns an internal server error
    Given the ship service fails when retrieving ships
    When all ships are requested
    Then the response status should be InternalServerError

  Scenario: Creating a ship without mandatory fields returns a bad request response
    Given a new ship without a code
    When the ship is created
    Then the response status should be BadRequest
