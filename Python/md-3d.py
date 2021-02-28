#!/usr/bin/python
# -*- coding: utf-8 -*-
# 
#######NOTE######
# The basis of this code comes from: 
# https://pythoninchemistry.org/sim_and_scat/molecular_dynamics/build_an_md
#
# The code have been changed slightly, but the main work can be contributed to
# pythoninchemistry.org.
#
# Reference:
#
# McCluskey, A. R.; Grant, J.; Symington, A. R.; Snow, T.; Doutch, J.; 
# Morgan, B. J.; Parker, S. C.; Edler, K. J. 10.5281/zenodo.2654373


from datetime import datetime

import numpy as np
import matplotlib.pyplot as plt
mass_of_argon = 39.948 # amu

def lj_force(r, epsilon, sigma):
    """
    Implementation of the Lennard-Jones potential
    to calculate the force of the interaction.

    Parameters
    ----------
    r: float
        Distance between two particles (Å)
    epsilon: float
        Potential energy at the equilibrium bond
        length (eV)
    sigma: float
        Distance at which the potential energy is
        zero (Å)

    Returns
    -------
    float
        Force of the van der Waals interaction (eV/Å)
    """
    abs_r = np.absolute(r)
    abs_sigma = np.absolute(sigma)
    ln_r = np.log(abs_r)
    ln_sigma = np.log(abs_sigma)
    mul_12 = 12 * ln_sigma
    mul_6 = 6 * ln_sigma
    mul_14 = 14 * ln_r
    mul_8 = 8 * ln_r
    exp_12 = np.exp(mul_12)
    exp_6 = np.exp(mul_6)
    exp_14 = np.exp(mul_14)
    exp_8 = np.exp(mul_8)
    div_12_14 = exp_12 / exp_14
    div_6_8 = exp_6 / exp_8
    mul_epsilon_12_14 = epsilon * div_12_14
    mul_epsilon_6_8 = epsilon * div_6_8
    mul_48 = 48 * mul_epsilon_12_14
    mul_24 = 24 * mul_epsilon_6_8
    min = mul_48 - mul_24
    return min

    # return ((48 * epsilon * ((np.exp(12 * np.log(np.absolute(sigma)))) / (np.exp(14 * np.log(np.absolute(r)))))) - (24 * epsilon * ((np.exp(6 * np.log(np.absolute(sigma)))) / (np.exp(8 * np.log(np.absolute(r)))))))



def get_accelerations(positions, dimensions):
    """
    Calculate the acceleration on each particle
    as a  result of each other particle.
    N.B. We use the Python convention of
    numbering from 0.

    Parameters
    ----------
    positions: ndarray of floats
        The positions, in a single dimension,
        for all of the particles

    Returns
    -------
    ndarray of floats
        The acceleration on each
        particle (eV/Åamu)
    """
    accel = np.zeros((dimensions, positions[0].size, positions[0].size), dtype=np.float64)
    for i in range(0, positions[0].size - 1):
        for j in range(i + 1, positions[0].size):
            r_x = positions[0][j] - positions[0][i]
            r_y = positions[1][j] - positions[1][i]
            r_z = positions[2][j] - positions[2][i]
            rmag = np.sqrt((r_x * r_x) + (r_y * r_y) + (r_z *r_z))
            force_scalar = lj_force(rmag, 0.0103, 3.4)
            force_x = force_scalar * r_x
            force_y = force_scalar * r_y
            force_z = force_scalar * r_z
            accel[0][i, j] = force_x / mass_of_argon
            accel[0][j, i] = - force_x / mass_of_argon
            accel[1][i, j] = force_y / mass_of_argon
            accel[1][j, i] = - force_y / mass_of_argon
            accel[2][i, j] = force_z / mass_of_argon
            accel[2][j, i] = - force_z / mass_of_argon
    a = np.sum(accel, axis=2)
    return a

def init_velocity(number_of_particles, dimensions):
    """
    Initialise the velocities for a series of
    particles.

    Parameters
    ----------
    number_of_particles: int
        Number of particles in the system

    Returns
    -------
    ndarray of floats
        Initial velocities for a series of
        particles (eVs/Åamu)
    """
    R = np.zeros((dimensions, number_of_particles), dtype=np.float64)
    return R


def update_pos(x, v, dt):
    """
    Update the particle positions.

    Parameters
    ----------
    x: ndarray of floats
        The positions of the particles in a
        single dimension
    v: ndarray of floats
        The velocities of the particles in a
        single dimension
    dt: float
        The timestep length

    Returns
    -------
    ndarray of floats:
        New positions of the particles in a single
        dimension
    """
    result = x + (v * dt)
    return result

def update_velo(v, a, dt):
    """
    Update the particle velocities.

    Parameters
    ----------
    v: ndarray of floats
        The velocities of the particles in a
        single dimension (eVs/Åamu)
    a: ndarray of floats
        The accelerations of the particles in a
        single dimension at the previous
        timestep (eV/Åamu)
    dt: float
        The timestep length

    Returns
    -------
    ndarray of floats:
        New velocities of the particles in a
        single dimension (eVs/Åamu)
    """
    result = v + (a * dt)
    return result

def run_md(dt, number_of_steps, x, dim):
    """
    Run a 2d MD simulation.

    Parameters
    ----------
    dt: float
        The timestep length (s)
    number_of_steps: int
        Number of iterations in the simulation
    x: ndarray of floats
        The initial positions of the particles in 2
        dimension (Å)

    Returns
    -------
    ndarray of floats
        The positions for all of the particles
        throughout theimport datetime simulation (Å)
    """
    positions = np.zeros((number_of_steps, dim, x[0].size), dtype=np.float64)

    v = init_velocity(x[0].size, dim)


    for i in range(number_of_steps):
        a = get_accelerations(x, dim)
        v = update_velo(v, a, dt)
        x = update_pos(x, v, dt)
        positions[i, :] = x
    return positions

x = np.arange(1, 21, dtype=np.float64)

space = np.float64([x,x,x])
dimensions = 3
loops = 2

start = datetime.now()
sim_pos = run_md(0.10, loops, space, dimensions)
end = datetime.now()
print (end - start)
print (np.__version__)
# for j in range(0,loops):
#     for k in range(0, dimensions):
#         print k, " - dimension:"
#         for i in range(0,space[0].size):
#             print sim_pos[j][k][i]
#     print "*** loop end ***"







# print sim_pos
# for i in range(sim_pos.shape[1]):
#     plt.plot(sim_pos[:, i], '.', label='atom {}'.format(i))
# plt.xlabel(r'Step')
# plt.ylabel(r'$x$-Position/')
# plt.legend(frameon=False)
# plt.show()
