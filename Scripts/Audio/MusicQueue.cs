// Copyright (C) by Upvoid Studios
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Engine.Audio;

namespace UpvoidMiner
{
    /// <summary>
    /// The MusicQueue contains a list of sounds that will be played one after another
    /// </summary>
    public class MusicQueue
    {

        Random random = new Random();

        private List<Sound> queue = new List<Sound>();

        private float minPause = 0;
        private float maxPause = 0;
        private bool repeatAll = true;

        private bool musicQueueStarted = false;
        private float currentPause = 0;

        public MusicQueue(float minPause = 0, float maxPause = 0, bool repeatAll = true)
        {
            this.minPause = minPause;
            this.maxPause = maxPause;
            this.repeatAll = repeatAll;
        }

        /// <summary>
        /// Add a sound to the end of the queue
        /// </summary>
        public void Add(Sound sound)
        {
            if(sound.Loop == true)
            {
                // Do not add looping sound (this is in no way the idea of a music queue)
                Console.WriteLine("Ignoring attempt to add looping sound to music queue.");
                return;
            }

            queue.Add(sound);
        }

        /// <summary>
        /// Removes the first occurence of the sound from the queue
        /// </summary>
        public void Remove(Sound sound)
        {
            queue.Remove(sound);

            if(sound.IsPlaying())
            {
                // We removed the first element, stop playing it
                sound.Stop();
            }
        }

        /// <summary>
        /// Update the musicqueue
        /// </summary>
        public void update(float _elapsedSeconds)
        {
            // Nothing to do here
            if (queue.Count == 0)
                return;

            Sound current = queue[0];

            if (!musicQueueStarted)
            {
                // First update call: play first sound
                current.Play();

                // Add a random pause
                currentPause = getRandomValue(minPause, maxPause);

                musicQueueStarted = true;
                return;
            }

            if (current.IsPlaying())
            {
                // Sound is currently playing, nothing to do here
                return;
            }

            // No sound is playing right now
            currentPause -= _elapsedSeconds;
            if (currentPause > 0)
            {
                // Still in pausing state...
                return;
            }


            // There is at least one sound enqueued and the first one is not playing 
            // and we are not pausing (anymore) right now, that means we should play a new sound
            // and setup the pause time for the time after that new sound has finished playing

            // Remove current sound
            queue.RemoveAt(0);

            if (repeatAll)
            {
                // ...and add it to the end
                queue.Add(current);
            }
            else
            {
                // In this case, the queue might be empty, check that
                if (queue.Count == 0)
                {
                    // Reset pause (Just to be sure)
                    currentPause = 0;
                    return;
                }
            }

            // Add a random pause
            currentPause = getRandomValue(minPause, maxPause);

            // Play the sound that now is the first one
            queue[0].Play();
        }

        private float getRandomValue(float a, float b)
        {
            if (a == b)
                return a;

            // Do not care for what value is larger than the other one...
            return a + (float)random.NextDouble() * Math.Abs(b - a);
        }
    }
}
